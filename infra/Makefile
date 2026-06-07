.PHONY: build helm-up helm-down compose-up compose-down clean

build:
	cd ../src && docker build -t ums-api:latest -f apps/ums.api/Ums.Presentation/Dockerfile .
	cd ../src && docker build -t ums-web:latest -f apps/ums.web-app/Dockerfile .

helm-up:
	helm upgrade --install ums ./local/kubernetes/helm/ums

helm-down:
	helm uninstall ums || true

compose-up:
	docker compose -f local/compose/docker-compose.yml up -d

compose-down:
	docker compose -f local/compose/docker-compose.yml down -v

clean: helm-down compose-down
	docker images | grep ums | awk '{print $$3}' | xargs -r docker rmi -f
