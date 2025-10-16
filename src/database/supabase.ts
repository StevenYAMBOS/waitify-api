import { createClient } from "@supabase/supabase-js";

const supabaseURL = process.env.SUPABASE_URL;
const supabaseAPIKey = process.env.SUPABASE_API_KEY;
export const supabase = createClient(supabaseURL, supabaseAPIKey);
