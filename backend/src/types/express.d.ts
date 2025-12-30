import { VerifiedClaims } from '@privy-io/server-auth'; // Or any if not needed

declare global {
  namespace Express {
    interface Request {
      user?: any; // Or better: VerifiedClaims from Privy if you import it
    }
  }
}