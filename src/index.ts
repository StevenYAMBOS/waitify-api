import express from "express";
import bodyParser from "body-parser";
import cookieParser from "cookie-parser";
import fs from "fs";
import path from "path";
import supabase from "./database/supabase";
import dotenv from "dotenv";
dotenv.config();

const port = process.env.SERVER_PORT || 3000;
const app = express();
app.use(bodyParser.urlencoded({ extended: true }));
app.use(cookieParser());

app.post("/signup", async (req, res) => {
  const { email, password } = req.body;
  const { user, error } = await supabase.auth.signUp({ email, password });

  if (error)
    return res.redirect(`/error.html?msg=${encodeURIComponent(error.message)}`);
  res.redirect("/signup_success.html");
});

app.post("/login", async (req, res) => {
  const { email, password } = req.body;
  const { data, error } = await supabase.auth.signInWithPassword({
    email,
    password,
  });

  if (error)
    return res.redirect(`/error.html?msg=${encodeURIComponent(error.message)}`);

  res.cookie("access_token", data.session.access_token, { httpOnly: true });
  res.redirect("/private");
});

app.get("/private", async (req, res) => {
  const token = req.cookies.access_token;
  if (!token) return res.redirect("/");

  const { data, error } = await supabase.auth.getUser(token);
  if (error) return res.redirect("/");

  const filePath = path.join(__dirname, "private.html");

  fs.readFile(filePath, "utf8", (err, html) => {
    if (err) {
      console.error("Error: private.html could not be loaded!", err);
      return res.status(500).send("Server error: private.html not found.");
    }

    const modifiedHtml = html.replace("{{userEmail}}", data.user.email);
    res.send(modifiedHtml);
  });
});

app.get("/logout", (req, res) => {
  res.clearCookie("access_token");
  res.redirect("/");
});

app.listen(port, () => {
  console.log(`L'application est lançé à l'adresse : http://localhost:${port}`);
});
