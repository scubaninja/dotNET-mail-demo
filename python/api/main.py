from fastapi import FastAPI, Depends, HTTPException
from sqlalchemy.orm import Session
import uvicorn

from core.database import get_db
from api.routes import public, admin

# Create FastAPI application
app = FastAPI(
    title="Tailwind Traders Mail Services API",
    description="Transactional and bulk email sending services for Tailwind Traders.",
    version="0.0.1",
    contact={
        "name": "Rob Conery, Aaron Wislang, and the Tailwind Traders Team",
        "url": "https://tailwindtraders.dev"
    },
    license_info={
        "name": "MIT",
        "url": "https://opensource.org/license/mit/"
    }
)

# Include routers
app.include_router(public.router)
app.include_router(admin.router, prefix="/admin")

@app.get("/")
async def root():
    """Root endpoint that redirects to docs"""
    return {"message": "Welcome to Tailwind Mail API. Visit /docs for documentation."}

if __name__ == "__main__":
    uvicorn.run("api.main:app", host="0.0.0.0", port=8000, reload=True)
