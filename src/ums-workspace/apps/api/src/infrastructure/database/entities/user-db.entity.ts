import {
  Entity,
  Column,
  PrimaryColumn,
  CreateDateColumn,
  UpdateDateColumn,
} from 'typeorm';

@Entity('users')
export class UserDbEntity {
  @PrimaryColumn('uuid')
  id!: string;

  @Column({ unique: true, length: 50 })
  username!: string;

  @Column({ unique: true, length: 150 })
  email!: string;

  @Column({ name: 'password_hash' })
  passwordHash!: string;

  @Column({ default: 'user', length: 20 })
  role!: string;

  @CreateDateColumn({ name: 'created_at' })
  createdAt!: Date;

  @UpdateDateColumn({ name: 'updated_at' })
  updatedAt!: Date;
}
