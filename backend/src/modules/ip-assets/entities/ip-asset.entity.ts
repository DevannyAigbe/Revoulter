// src/modules/ip-assets/entities/ip-asset.entity.ts
import { 
  Entity, PrimaryGeneratedColumn, Column, CreateDateColumn, 
  UpdateDateColumn, ManyToOne, JoinColumn 
} from 'typeorm';
import { User } from '../../../users/entities/user.entity';

@Entity('ip_assets')
export class IPAsset {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column()
  title: string;

  @Column()
  fileName: string;

  @Column()
  filePath: string;

  @Column()
  fileType: string;

  @Column('int')
  fileSize: number;

  @Column({ type: 'text', nullable: true })
  arweaveId: string | null;

  @Column({ type: 'text', nullable: true })
  ipAssetId: string | null;

  @Column({ type: 'text', nullable: true })
  transactionHash: string | null;

  @Column('jsonb', { nullable: true })
  metadata: any;

  @Column({ default: 'draft' })
  status: string;

  @Column({ type: 'timestamptz', nullable: true })
  publishedAt: Date | null;

  @ManyToOne(() => User, (user) => user.ipAssets, { onDelete: 'CASCADE' })
  @JoinColumn({ name: 'userId' })
  user: User;

  @Column()
  userId: string;

  @CreateDateColumn()
  createdAt: Date;

  @UpdateDateColumn()
  updatedAt: Date;
}