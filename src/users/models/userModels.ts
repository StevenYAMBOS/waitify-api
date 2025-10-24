interface User {
  id: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  companyName: string;
  isActive: boolean;
  subscriptionStatus: string;
  trialEndsAt: Date | string;
  subscriptionPlanId: string;
  googleId: string;
  profilePicture: string;
  authProvider: string;
  createdAt: Date;
  updatedAt: Date;
  lastLogin: Date;
}

interface GetUser {
  User: User;
}

export { User, GetUser };
