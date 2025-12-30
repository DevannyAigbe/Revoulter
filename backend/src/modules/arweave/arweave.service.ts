// src/modules/arweave/arweave.service.ts
import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import Arweave from 'arweave';

@Injectable()
export class ArweaveService {
  private readonly logger = new Logger(ArweaveService.name);
  private arweave: any;
  private isAvailable: boolean = false;

  constructor(private configService: ConfigService) {
    this.initialize();
  }

  private initialize() {
    try {
      this.arweave = Arweave.init({
        host: this.configService.get('arweave.host'),
        port: this.configService.get('arweave.port'),
        protocol: this.configService.get('arweave.protocol'),
      });
      this.isAvailable = true;
      this.logger.log('Arweave service initialized');
    } catch (error) {
      this.logger.warn('Arweave service not available, will use mock mode');
      this.isAvailable = false;
    }
  }

  async uploadFile(file: Express.Multer.File): Promise<string> {
    if (!this.isAvailable) {
      throw new Error('Arweave service not available');
    }

    try {
      const walletKey = this.configService.get('arweave.walletKey');
      if (!walletKey) {
        throw new Error('Arweave wallet key not configured');
      }

      const transaction = await this.arweave.createTransaction({
        data: file.buffer,
      }, walletKey);

      await this.arweave.transactions.sign(transaction, walletKey);
      const response = await this.arweave.transactions.post(transaction);

      if (response.status !== 200) {
        throw new Error(`Arweave upload failed: ${response.statusText}`);
      }

      return transaction.id;
    } catch (error) {
      this.logger.error('Arweave upload failed:', error);
      throw error;
    }
  }

  async mockUpload(file: Express.Multer.File): Promise<string> {
    // Generate a mock Arweave transaction ID
    const mockId = `mock-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    this.logger.log(`Mock Arweave upload: ${mockId}`);
    return mockId;
  }

  isServiceAvailable(): boolean {
    return this.isAvailable;
  }
}