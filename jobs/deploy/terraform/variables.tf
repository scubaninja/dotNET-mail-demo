variable "env_name" {
  default = "my-environment"
}

variable "app_name" {
  default = "my-container-app"
}

variable "app_image" {
  default = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "job_name" {
  default = "my-container-job"
}

variable "job_image" {
  default = "ghcr.io/tailwind-traders-dev/jobs:latest"
}

variable "service_bus_connection" {
  default = ""
}

variable "app_queue_name" {
  default = "tailwind"
}

variable "job_queue_name" {
  default = "tailwind-job"
}

variable "location" {
  default = "eastus"
}
