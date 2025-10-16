"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = __importDefault(require("express"));
const dotenv_1 = __importDefault(require("dotenv"));
dotenv_1.default.config();
const app = (0, express_1.default)();
const port = process.env.SERVER_PORT;
app.get("/", (req, res) => {
    res.send("Hello World!");
});
app.listen(port, () => {
    return console.log(`L'application est lançé à l'adresse : http://localhost:${port}`);
});
//# sourceMappingURL=index.js.map