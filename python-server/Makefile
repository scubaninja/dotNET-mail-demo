# Makefile for Tailwind Traders Mail Service (Python)

.PHONY: help install test test-unit test-integration test-accessibility test-coverage lint format security clean

# Default target
help:
	@echo "Available targets:"
	@echo "  install              - Install dependencies"
	@echo "  test                 - Run all tests"
	@echo "  test-unit            - Run unit tests only"
	@echo "  test-integration     - Run integration tests only"
	@echo "  test-accessibility   - Run accessibility tests only"
	@echo "  test-coverage        - Run tests with coverage report"
	@echo "  lint                 - Run linters (flake8, pylint)"
	@echo "  format               - Format code with black and isort"
	@echo "  security             - Run security checks"
	@echo "  clean                - Clean up generated files"

# Install dependencies
install:
	pip install -r requirements-test.txt
	pip install -e .

# Run all tests
test:
	pytest -v

# Run unit tests only
test-unit:
	pytest -v -m unit

# Run integration tests only
test-integration:
	pytest -v -m integration

# Run accessibility tests only
test-accessibility:
	pytest -v -m accessibility

# Run tests with coverage
test-coverage:
	pytest --cov=app --cov-report=html --cov-report=term-missing --cov-report=xml
	@echo "Coverage report generated in htmlcov/index.html"

# Run tests in parallel
test-parallel:
	pytest -n auto -v

# Run linters
lint:
	@echo "Running flake8..."
	flake8 app/ tests/
	@echo "Running pylint..."
	pylint app/ tests/
	@echo "Running mypy..."
	mypy app/

# Format code
format:
	@echo "Running black..."
	black app/ tests/
	@echo "Running isort..."
	isort app/ tests/

# Check formatting without making changes
format-check:
	black --check app/ tests/
	isort --check app/ tests/

# Run security checks
security:
	@echo "Running bandit..."
	bandit -r app/
	@echo "Checking dependencies..."
	safety check

# Clean up generated files
clean:
	find . -type d -name __pycache__ -exec rm -rf {} + 2>/dev/null || true
	find . -type f -name "*.pyc" -delete
	find . -type d -name "*.egg-info" -exec rm -rf {} + 2>/dev/null || true
	rm -rf .pytest_cache
	rm -rf htmlcov
	rm -rf .coverage
	rm -rf coverage.xml
	rm -rf dist
	rm -rf build

# Run full CI pipeline
ci: format-check lint security test-coverage
	@echo "CI pipeline completed successfully!"

# Development setup
dev-setup: install
	@echo "Installing pre-commit hooks..."
	# pip install pre-commit
	# pre-commit install
	@echo "Development setup complete!"

# Run tests with watch mode (requires pytest-watch)
test-watch:
	ptw -- -v

# Generate test report
test-report:
	pytest --html=report.html --self-contained-html

# Run specific test file
test-file:
	@read -p "Enter test file path: " filepath; \
	pytest -v $$filepath

# Show test coverage in terminal
coverage:
	coverage report -m

# Open coverage report in browser
coverage-html:
	coverage html
	python -m http.server --directory htmlcov 8080
