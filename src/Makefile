.PHONY: up down build rebuild logs clean

up:
	docker compose up -d

down:
	docker compose down

build:
	docker compose build

rebuild:
	docker compose down
	docker compose build --no-cache
	docker compose up -d

logs:
	docker compose logs -f

logs-api:
	docker compose logs -f ums-api

logs-web:
	docker compose logs -f ums-web

logs-db:
	docker compose logs -f postgres

clean:
	docker compose down -v --rmi all

dev:
	docker compose -f docker-compose.yml up -d

prod:
	docker compose -f docker-compose.yml up -d --build
