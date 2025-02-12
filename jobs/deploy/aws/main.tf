provider "aws" {
  region = "us-east-1"
}

resource "aws_iam_role" "managed_identity" {
  name = "${var.resource_group_name}-identity"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ec2.amazonaws.com"
      }
    }]
  })
}

resource "aws_ecr_repository" "container_registry" {
  name = "acr${random_string.rand.result}"
}

resource "aws_sqs_queue" "service_bus" {
  name = "servicebus${random_string.rand.result}"
}

resource "aws_s3_bucket" "storage_account" {
  bucket = "storage${random_string.rand.result}"
}

resource "aws_secretsmanager_secret" "key_vault" {
  name = "keyvault${random_string.rand.result}"
}

resource "aws_secretsmanager_secret_version" "key_vault_secret" {
  secret_id = aws_secretsmanager_secret.key_vault.id
  secret_string = var.secret_value
}

resource "random_string" "rand" {
  length = 6
  special = false
}

variable "resource_group_name" {
  type = string
}

variable "secret_value" {
  type = string
  default = uuid()
}

variable "deploy_postgres" {
  type = bool
  default = true
}

module "postgres" {
  source = "./postgres"
  enabled = var.deploy_postgres
}

module "postgres_administrator" {
  source = "./postgres-admin"
  enabled = var.deploy_postgres
  postgres_name = module.postgres.postgres_name
  principal_id = aws_iam_role.managed_identity.id
  principal_name = aws_iam_role.managed_identity.name
}
