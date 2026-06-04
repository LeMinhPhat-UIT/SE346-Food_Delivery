# Sử dụng base image Node.js nhẹ nhàng
FROM node:20-alpine

WORKDIR /app

ARG DATABASE_URL=postgresql://localhost:5432/postgres
ENV DATABASE_URL=${DATABASE_URL}

COPY package*.json ./

RUN npm install

COPY prisma ./prisma/
COPY . .

RUN npx prisma generate
RUN npm run build

EXPOSE 8080

CMD ["sh", "-c", "npx prisma db push && npm start"]
