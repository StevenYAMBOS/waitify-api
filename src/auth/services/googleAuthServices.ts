// src/auth/services/googleAuthService.ts

import { UserRepository } from "../../users/repositories/userRepositories";
import { v4 as uuidv4 } from "uuid";
import { GOOGLE_API, HTTP_METHODS } from "../../config/constants";
import { GoogleTokenResponse, GoogleUserInfo } from "../models/authModels";

export class GoogleAuthService {
  constructor(private userRepository: UserRepository) {}

  /**
   * Génère l'URL de consentement Google OAuth
   */
  getConsentScreenUrl(state: string): string {
    const scopes = GOOGLE_API.OAUTH_SCOPES.join(" ");
    const params = new URLSearchParams({
      client_id: GOOGLE_API.CLIENT_ID,
      redirect_uri: GOOGLE_API.REDIRECT_URL,
      access_type: "offline",
      response_type: "code",
      state: state,
      scope: scopes,
      prompt: "consent", // Force l'affichage du consentement
    });

    return `${GOOGLE_API.OAUTH_URL}?${params.toString()}`;
  }

  /**
   * Échange le code d'autorisation contre un access token
   */
  async exchangeCodeForTokens(
    code: string
  ): Promise<GoogleTokenResponse | null> {
    try {
      const data = {
        code,
        client_id: GOOGLE_API.CLIENT_ID,
        client_secret: GOOGLE_API.CLIENT_SECRET,
        redirect_uri: GOOGLE_API.REDIRECT_URL,
        grant_type: "authorization_code",
      };

      const response = await fetch(GOOGLE_API.ACCESS_TOKEN_URL, {
        method: HTTP_METHODS.POST,
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const errorData = await response.json();
        console.error("Erreur lors de l'échange du code", {
          status: response.status,
          error: errorData,
        });
        return null;
      }

      const tokens: GoogleTokenResponse = await response.json();

      return tokens;
    } catch (error) {
      console.error("Exception lors de l'échange du code", {
        error: error instanceof Error ? error.message : error,
      });
      return null;
    }
  }

  async getUserInfo(idToken: string): Promise<GoogleUserInfo | null> {
    try {
      const response = await fetch(
        `${GOOGLE_API.TOKEN_INFO_URL}?id_token=${idToken}`
      );

      if (!response.ok) {
        const errorData = await response.json();
        console.error("Erreur lors de la récupération des infos", {
          status: response.status,
          error: errorData,
        });
        return null;
      }

      const userInfo: GoogleUserInfo = await response.json();

      return userInfo;
    } catch (error) {
      console.error("Exception lors de la récupération des infos", {
        error: error instanceof Error ? error.message : error,
      });
      return null;
    }
  }

  async findOrCreateUser(googleUser: GoogleUserInfo) {
    let user = await this.userRepository.findByEmail(googleUser.email);

    if (user) {
      // Mettre à jour la photo de profil si elle a changé
      if (user.profilePicture !== googleUser.picture) {
        await this.userRepository.updateProfilePicture(
          user.id,
          googleUser.picture
        );
      }

      return user;
    }

    // Créer un nouvel utilisateur
    const uuid = uuidv4();
    const date = new Date();

    user = await this.userRepository.create({
      id: uuid,
      email: googleUser.email,
      password: "", // Pas de mot de passe pour OAuth, Google ne le partage pas (même pas crypté)
      created_at: date,
      updated_at: date,
      profile_picture: googleUser.picture,
      google_id: googleUser.sub, // Stocker l'ID Google
    });

    return user;
  }
}
