import { createClient } from "@supabase/supabase-js";

const databaseUrl = process.env.DATABASE_URL;
const databaseKey = process.env.DATABASE_API_KEY;
export const database = createClient(databaseUrl, databaseKey);
