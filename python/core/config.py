import os
from typing import Dict, Optional

class Config:
    """Configuration handler similar to the .NET Viper.Config()"""
    
    _instance = None
    _config: Dict[str, str] = {}
    
    def __new__(cls, environment=None):
        if cls._instance is None:
            cls._instance = super(Config, cls).__new__(cls)
            cls._instance._load_config(environment)
        return cls._instance
    
    def _load_config(self, environment: Optional[str] = None):
        """Load configuration from environment variables"""
        # Default configuration
        self._config = {
            "DATABASE_URL": os.getenv("DATABASE_URL", "postgresql://postgres:postgres@localhost/mail_demo"),
            "SMTP_HOST": os.getenv("SMTP_HOST", "localhost"),
            "SMTP_USER": os.getenv("SMTP_USER", ""),
            "SMTP_PASSWORD": os.getenv("SMTP_PASSWORD", ""),
            "DEFAULT_FROM": os.getenv("DEFAULT_FROM", "test@tailwind.dev"),
            "AZURE_OPENAI_ENDPOINT": os.getenv("AZURE_OPENAI_ENDPOINT", ""),
            "AZURE_OPENAI_API_KEY": os.getenv("AZURE_OPENAI_API_KEY", ""),
            "SEND_WORKER": os.getenv("SEND_WORKER", "local"),
        }
        
        # Load environment-specific config if needed
        if environment:
            # In a real app, this might load from a config file
            pass
    
    def get(self, key: str) -> str:
        """Get a configuration value by key"""
        return self._config.get(key, "")

def get_config(environment=None) -> Config:
    """Helper function to get config instance"""
    return Config(environment)
