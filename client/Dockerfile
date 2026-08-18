FROM node:24-alpine AS build

WORKDIR /app

COPY client/package*.json ./

RUN npm ci

COPY client/ ./

RUN npm run build


FROM nginx:alpine AS final

COPY --from=build \
    /app/dist \
    /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]