// src/database/migrations/0001-create-tables.ts
import { MigrationInterface, QueryRunner } from 'typeorm';

export class CreateTables0001 implements MigrationInterface {
  name = 'CreateTables0001'

  public async up(queryRunner: QueryRunner): Promise<void> {
    // Create users table
    await queryRunner.query(`
      CREATE TABLE users (
        "privyId" VARCHAR PRIMARY KEY,
        "walletAddress" VARCHAR,
        "email" VARCHAR,
        "userMetadata" JSONB,
        "externalServicesEnabled" BOOLEAN DEFAULT FALSE,
        "createdAt" TIMESTAMP DEFAULT NOW(),
        "updatedAt" TIMESTAMP DEFAULT NOW()
      )
    `);

    // Create ip_assets table
    await queryRunner.query(`
      CREATE TABLE ip_assets (
        "id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        "title" VARCHAR NOT NULL,
        "fileName" VARCHAR NOT NULL,
        "filePath" VARCHAR NOT NULL,
        "fileType" VARCHAR NOT NULL,
        "fileSize" INTEGER NOT NULL,
        "arweaveId" VARCHAR,
        "ipAssetId" VARCHAR,
        "transactionHash" VARCHAR,
        "metadata" JSONB,
        "status" VARCHAR NOT NULL DEFAULT 'draft',
        "publishedAt" TIMESTAMP,
        "userId" VARCHAR NOT NULL REFERENCES users("privyId"),
        "createdAt" TIMESTAMP DEFAULT NOW(),
        "updatedAt" TIMESTAMP DEFAULT NOW()
      )
    `);

    // Create indexes
    await queryRunner.query(`
      CREATE INDEX idx_ip_assets_user_id ON ip_assets("userId");
      CREATE INDEX idx_ip_assets_status ON ip_assets("status");
    `);
  }

  public async down(queryRunner: QueryRunner): Promise<void> {
    await queryRunner.query(`DROP TABLE ip_assets`);
    await queryRunner.query(`DROP TABLE users`);
  }
}