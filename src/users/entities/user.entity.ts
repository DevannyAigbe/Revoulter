// src/modules/users/entities/user.entity.ts
import { Entity, PrimaryColumn, Column, CreateDateColumn, OneToMany } from 'typeorm';
import { IPAsset } from '../../modules/ip-assets/entities/ip-asset.entity';

@Entity('users')
export class User {
  @PrimaryColumn()
  privyId: string;

  @Column({ type: 'text', nullable: true })
  walletAddress: string | null;

  @Column({ type: 'text', nullable: true })
  email: string | null;

  @Column('jsonb', { nullable: true })
  userMetadata: any;

  @Column({ default: false })
  externalServicesEnabled: boolean;

  @OneToMany(() => IPAsset, (ipAsset) => ipAsset.user)
  ipAssets: IPAsset[];

  @CreateDateColumn()
  createdAt: Date;

  @CreateDateColumn()
  updatedAt: Date;
}