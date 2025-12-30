// src/modules/story-protocol/story-protocol.service.ts
import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

@Injectable()
export class StoryProtocolService {
  private readonly logger = new Logger(StoryProtocolService.name);
  private isAvailable: boolean = false;
  private walletAddress: string | null = null;

  constructor(private configService: ConfigService) {
    this.initialize();
  }

  private async initialize() {
    try {
      // Check if we have the required configuration for Story Protocol
      const iliadRpc = this.configService.get('storyProtocol.iliadRpc');
      const spgContract = this.configService.get('storyProtocol.spgContract');
      const privateKey = this.configService.get('storyProtocol.privateKey');
      
      if (iliadRpc && spgContract && privateKey) {
        // Test the RPC connection
        const response = await fetch(iliadRpc, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            jsonrpc: '2.0',
            method: 'eth_chainId',
            params: [],
            id: 1,
          }),
        });
        
        if (response.ok) {
          this.isAvailable = true;
          // Derive wallet address from private key (simplified - in reality use viem)
          this.walletAddress = this.deriveAddressFromPrivateKey(privateKey);
          this.logger.log('Story Protocol service initialized');
        }
      }
    } catch (error) {
      this.logger.warn('Story Protocol service not available, will use mock mode');
      this.isAvailable = false;
    }
  }

  async registerIPAsset(
    arweaveId: string,
    title: string,
    userWalletAddress: string,
  ): Promise<{ ipId: string; txHash: string }> {
    if (!this.isAvailable || this.configService.get('app.mode') === 'mock-only') {
      return this.mockRegisterIPAsset(arweaveId, title, userWalletAddress);
    }

    try {
      // Real Story Protocol integration would go here
      // This is a placeholder for the actual implementation
      
      this.logger.log(`Attempting to register IP asset for ${userWalletAddress}`);
      this.logger.log(`Arweave ID: ${arweaveId}`);
      this.logger.log(`Title: ${title}`);
      
      // TODO: Implement actual Story Protocol SDK integration
      // const client = StoryClient.newClient(config);
      // const response = await client.ipAsset.register({ ... });
      
      // For now, fall back to mock
      return this.mockRegisterIPAsset(arweaveId, title, userWalletAddress);
      
    } catch (error) {
      this.logger.error('Story Protocol registration failed:', error);
      this.logger.warn('Falling back to mock registration');
      return this.mockRegisterIPAsset(arweaveId, title, userWalletAddress);
    }
  }

  async mockRegisterIPAsset(
    arweaveId: string,
    title: string,
    userWalletAddress: string,
  ): Promise<{ ipId: string; txHash: string }> {
    // Generate mock Story Protocol data
    const mockIpId = `ip:testnet:${Date.now()}:${Math.random().toString(36).substr(2, 9)}`;
    const mockTxHash = `0x${Date.now().toString(16)}${Math.random().toString(36).substr(2, 48)}`;
    
    this.logger.log(`Mock Story Protocol registration for ${userWalletAddress}`);
    this.logger.log(`Generated mock IP ID: ${mockIpId}`);
    this.logger.log(`Generated mock TX hash: ${mockTxHash}`);
    
    return {
      ipId: mockIpId,
      txHash: mockTxHash,
    };
  }

  isServiceAvailable(): boolean {
    return this.isAvailable;
  }

  async getServiceStatus(): Promise<{
    available: boolean;
    mode: 'real' | 'mock';
    walletAddress?: string;
  }> {
    return {
      available: this.isAvailable,
      mode: this.isAvailable ? 'real' : 'mock',
      walletAddress: this.walletAddress || undefined,
    };
  }

  private deriveAddressFromPrivateKey(privateKey: string): string {
    // Simplified address derivation
    // In a real implementation, use viem or ethers to derive the address
    // This is just a placeholder
    try {
      // Remove 0x prefix if present
      const cleanKey = privateKey.startsWith('0x') ? privateKey.slice(2) : privateKey;
      
      // Use last 40 characters (simplified - not secure!)
      // Real implementation would use proper elliptic curve cryptography
      const address = `0x${cleanKey.slice(-40).toLowerCase()}`;
      
      // Ensure it's a valid Ethereum address format
      if (/^0x[a-fA-F0-9]{40}$/.test(address)) {
        return address;
      }
      return `0x${'0'.repeat(40)}`; // Fallback
    } catch {
      return `0x${'0'.repeat(40)}`;
    }
  }

  // Optional: Method to sync a mock asset to real Story Protocol later
  async syncMockToReal(
    mockIpId: string,
    arweaveId: string,
    userWalletAddress: string,
  ): Promise<{ ipId: string; txHash: string }> {
    this.logger.log(`Syncing mock asset ${mockIpId} to real Story Protocol`);
    
    if (!this.isAvailable) {
      throw new Error('Story Protocol service not available');
    }
    
    // This would contain the logic to register an existing mock asset
    // Similar to registerIPAsset but might handle existing references
    return this.registerIPAsset(arweaveId, 'Synced Asset', userWalletAddress);
  }
}