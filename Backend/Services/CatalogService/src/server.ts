import express from "express";
import cors from "cors";
import catalogRoutes from "./presentation/routes";

const app = express();
const PORT = process.env.PORT || 8084;

app.use(cors());
app.use(express.json());

app.use("/api/catalog", catalogRoutes);

app.get("/health", (req, res) => {
    res.status(200).send('Catalog Service is running healthy!');
});

app.listen(PORT, () => {
  console.log(`Catalog Service is running on port ${PORT}`);
});