FROM node:20-alpine

WORKDIR /app

ARG DATABASE_URL=postgresql://localhost:5432/postgres
ARG DIRECT_URL=postgresql://localhost:5432/postgres
ENV DATABASE_URL=${DATABASE_URL}
ENV DIRECT_URL=${DIRECT_URL}

COPY package*.json ./

RUN npm install

COPY prisma ./prisma/
COPY . .

RUN npx prisma generate

RUN npm run build

EXPOSE 8080

CMD ["npm", "run", "start"]
