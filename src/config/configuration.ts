// src/config/configuration.ts
export default () => ({
  port: parseInt(process.env.PORT || '3000', 10),
  database: {
    host: process.env.DATABASE_HOST || 'localhost',
    port: parseInt(process.env.DATABASE_PORT || '5432', 10),
    username: process.env.DATABASE_USER || 'postgres',
    password: process.env.DATABASE_PASSWORD || 'password',
    database: process.env.DATABASE_NAME || 'ip_platform',
  },
  privy: {
    appId: process.env.PRIVY_APP_ID || '',
    appSecret: process.env.PRIVY_APP_SECRET || '',
  },
  arweave: {
    host: process.env.ARWEAVE_HOST || 'arweave.net',
    port: parseInt(process.env.ARWEAVE_PORT || '443', 10),
    protocol: process.env.ARWEAVE_PROTOCOL || 'https',
    walletKey: process.env.ARWEAVE_WALLET_KEY || '',
  },
  storyProtocol: {
    iliadRpc: process.env.ILIAD_RPC || 'https://testnet.storyrpc.io',
    spgContract: process.env.SPG_CONTRACT || '',
  },
  app: {
    mode: process.env.APP_MODE || 'hybrid', // 'external-only', 'mock-only', 'hybrid'
    uploadDir: process.env.UPLOAD_DIR || './uploads',
    maxFileSize: parseInt(process.env.MAX_FILE_SIZE || '10485760', 10), // 10MB
  },
});