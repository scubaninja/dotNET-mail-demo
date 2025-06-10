import asyncio
import httpx
from typing import Optional, Dict
from core.config import get_config

class Chat:
    """AI Chat service for generating content"""
    
    def __init__(self, config=None):
        self.config = config or get_config()
        self.endpoint = self.config.get("AZURE_OPENAI_ENDPOINT")
        self.api_key = self.config.get("AZURE_OPENAI_API_KEY")
    
    async def prompt(self, prompt: str) -> str:
        """Send a prompt to the Azure OpenAI service and get a response"""
        if not prompt:
            raise ValueError("Prompt cannot be empty")
            
        if not self.endpoint or not self.api_key:
            return f"AI configuration not set up. Prompt was: {prompt}"
            
        # Prepare the API request
        headers = {
            "Content-Type": "application/json",
            "api-key": self.api_key
        }
        
        payload = {
            "messages": [
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.7,
            "max_tokens": 800
        }
        
        deployment_name = "gpt-4"
        url = f"{self.endpoint}/openai/deployments/{deployment_name}/chat/completions?api-version=2023-05-15"
        
        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(url, headers=headers, json=payload)
                response.raise_for_status()
                data = response.json()
                return data["choices"][0]["message"]["content"]
        except Exception as e:
            # In a real app, log the error
            return f"Error communicating with AI service: {str(e)}"
