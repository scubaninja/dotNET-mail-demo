-- GDPR Compliance Migration Script
-- Add new columns to contacts table for GDPR compliance

ALTER TABLE mail.contacts ADD COLUMN IF NOT EXISTS consent_timestamp timestamptz;
ALTER TABLE mail.contacts ADD COLUMN IF NOT EXISTS privacy_policy_version text DEFAULT '1.0';
ALTER TABLE mail.contacts ADD COLUMN IF NOT EXISTS consent_source text DEFAULT 'Legacy';
ALTER TABLE mail.contacts ADD COLUMN IF NOT EXISTS consent_ip_address text;
ALTER TABLE mail.contacts ADD COLUMN IF NOT EXISTS data_retention_expiry timestamptz;
ALTER TABLE mail.contacts ADD COLUMN IF NOT EXISTS deletion_requested boolean DEFAULT false;

-- Mark existing contacts as legacy with NULL consent timestamp to indicate we need to re-collect consent
UPDATE mail.contacts SET consent_source = 'Legacy', privacy_policy_version = '1.0' WHERE consent_timestamp IS NULL;

-- Create a new table to store data processing records for compliance
CREATE TABLE IF NOT EXISTS mail.data_processing_activities (
  id serial primary key,
  contact_id int REFERENCES mail.contacts(id),
  activity_type text NOT NULL, -- e.g., 'access', 'modify', 'delete', 'export'
  description text NOT NULL,
  performed_by text NOT NULL, -- user or system that performed the action
  performed_at timestamptz NOT NULL DEFAULT now(),
  ip_address text
);

-- Create view to help with data subject access requests
CREATE OR REPLACE VIEW mail.contact_data_view AS
SELECT 
  c.id,
  c.name,
  c.email,
  c.subscribed,
  c.created_at,
  c.consent_timestamp,
  c.privacy_policy_version,
  c.consent_source,
  c.data_retention_expiry,
  string_agg(DISTINCT t.name, ', ') as tags,
  (SELECT COUNT(*) FROM mail.activity a WHERE a.contact_id = c.id) as activity_count
FROM mail.contacts c
LEFT JOIN mail.tagged tg ON c.id = tg.contact_id
LEFT JOIN mail.tags t ON tg.tag_id = t.id
GROUP BY c.id;

-- Create function to anonymize user data
CREATE OR REPLACE FUNCTION mail.anonymize_contact(contact_id int)
RETURNS void AS $$
BEGIN
  UPDATE mail.contacts 
  SET 
    name = 'Anonymized User', 
    email = 'anonymized-' || md5(random()::text) || '@example.com',
    consent_ip_address = NULL
  WHERE id = contact_id;
  
  INSERT INTO mail.data_processing_activities
    (contact_id, activity_type, description, performed_by)
  VALUES
    (contact_id, 'anonymize', 'User data anonymized due to retention policy', 'system');
END;
$$ LANGUAGE plpgsql;

-- Create function to delete user data (right to be forgotten)
CREATE OR REPLACE FUNCTION mail.delete_contact_data(contact_id int)
RETURNS void AS $$
BEGIN
  -- First log the deletion
  INSERT INTO mail.data_processing_activities
    (contact_id, activity_type, description, performed_by)
  VALUES
    (contact_id, 'delete', 'User data deleted per right to be forgotten request', 'system');
    
  -- Delete related data
  DELETE FROM mail.activity WHERE contact_id = contact_id;
  DELETE FROM mail.tagged WHERE contact_id = contact_id;
  DELETE FROM mail.subscriptions WHERE contact_id = contact_id;
  
  -- Finally delete the contact
  DELETE FROM mail.contacts WHERE id = contact_id;
END;
$$ LANGUAGE plpgsql;

-- Create index on email to improve GDPR search performance
CREATE INDEX IF NOT EXISTS idx_contacts_email ON mail.contacts(email);
