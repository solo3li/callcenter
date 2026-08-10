FROM golang:1.22 AS build
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN go build -o ai-worker main.go

FROM debian:bookworm-slim
WORKDIR /app
COPY --from=build /app/ai-worker /app/ai-worker
ENTRYPOINT ["/app/ai-worker"]
