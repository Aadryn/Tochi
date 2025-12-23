---
description: TypeScript Patterns - Design patterns, SOLID, Factory, Repository, Singleton, Observer
name: TypeScript_Patterns
applyTo: "**/*.ts"
---

# TypeScript Design Patterns

Guide complet des design patterns en TypeScript avec typage fort.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** le pattern Singleton sans raison valable (difficile à tester)
- **Ne crée jamais** de god classes (classes qui font tout)
- **N'utilise jamais** l'héritage pour partager du code (utiliser composition)
- **Ne mélange jamais** plusieurs responsabilités dans une classe
- **N'ignore jamais** les principes SOLID au profit de "rapidité"

## ✅ À FAIRE

- **Utilise toujours** la composition plutôt que l'héritage
- **Utilise toujours** des interfaces pour les abstractions
- **Applique toujours** le principe de responsabilité unique
- **Utilise toujours** l'injection de dépendances
- **Documente toujours** pourquoi un pattern est utilisé

## 🏭 Creational Patterns

### Factory Pattern

```typescript
/ types/product.types.ts
interface Product {
  readonly id: string;
  readonly name: string;
  readonly price: number;
  calculateDiscount(percentage: number): number;
}

type ProductType = 'physical' | 'digital' | 'subscription';

/ factories/product.factory.ts
class PhysicalProduct implements Product {
  constructor(
    public readonly id: string,
    public readonly name: string,
    public readonly price: number,
    public readonly weight: number,
  ) {}

  calculateDiscount(percentage: number): number {
    return this.price * (1 - percentage / 100);
  }
}

class DigitalProduct implements Product {
  constructor(
    public readonly id: string,
    public readonly name: string,
    public readonly price: number,
    public readonly downloadUrl: string,
  ) {}

  calculateDiscount(percentage: number): number {
    / Les produits digitaux ont une réduction max de 50%
    const maxDiscount = Math.min(percentage, 50);
    return this.price * (1 - maxDiscount / 100);
  }
}

class SubscriptionProduct implements Product {
  constructor(
    public readonly id: string,
    public readonly name: string,
    public readonly price: number,
    public readonly billingCycle: 'monthly' | 'yearly',
  ) {}

  calculateDiscount(percentage: number): number {
    / Pas de réduction sur les abonnements
    return this.price;
  }
}

/ ✅ Factory avec type guards et validation
interface ProductDTO {
  id: string;
  name: string;
  price: number;
  type: ProductType;
  weight?: number;
  downloadUrl?: string;
  billingCycle?: 'monthly' | 'yearly';
}

class ProductFactory {
  static create(dto: ProductDTO): Product {
    switch (dto.type) {
      case 'physical':
        if (dto.weight === undefined) {
          throw new Error('Physical products require weight');
        }
        return new PhysicalProduct(dto.id, dto.name, dto.price, dto.weight);

      case 'digital':
        if (!dto.downloadUrl) {
          throw new Error('Digital products require downloadUrl');
        }
        return new DigitalProduct(dto.id, dto.name, dto.price, dto.downloadUrl);

      case 'subscription':
        if (!dto.billingCycle) {
          throw new Error('Subscription products require billingCycle');
        }
        return new SubscriptionProduct(dto.id, dto.name, dto.price, dto.billingCycle);

      default:
        throw new Error(`Unknown product type: ${dto.type satisfies never}`);
    }
  }
}

/ Usage
const product = ProductFactory.create({
  id: '123',
  name: 'E-book TypeScript',
  price: 29.99,
  type: 'digital',
  downloadUrl: 'https:/example.com/download/123',
});
```

### Abstract Factory

```typescript
/ interfaces/ui.interfaces.ts
interface Button {
  render(): string;
  onClick(handler: () => void): void;
}

interface Input {
  render(): string;
  getValue(): string;
  setValue(value: string): void;
}

interface Modal {
  open(): void;
  close(): void;
  setContent(content: string): void;
}

/ ✅ Abstract Factory Interface
interface UIFactory {
  createButton(label: string): Button;
  createInput(placeholder: string): Input;
  createModal(title: string): Modal;
}

/ Material Design Implementation
class MaterialButton implements Button {
  constructor(private label: string) {}

  render(): string {
    return `<button class="mdc-button">${this.label}</button>`;
  }

  onClick(handler: () => void): void {
    / Material click implementation
  }
}

class MaterialInput implements Input {
  private value = '';

  constructor(private placeholder: string) {}

  render(): string {
    return `<input class="mdc-text-field" placeholder="${this.placeholder}" />`;
  }

  getValue(): string {
    return this.value;
  }

  setValue(value: string): void {
    this.value = value;
  }
}

class MaterialModal implements Modal {
  private content = '';
  private isOpen = false;

  constructor(private title: string) {}

  open(): void {
    this.isOpen = true;
  }

  close(): void {
    this.isOpen = false;
  }

  setContent(content: string): void {
    this.content = content;
  }
}

class MaterialUIFactory implements UIFactory {
  createButton(label: string): Button {
    return new MaterialButton(label);
  }

  createInput(placeholder: string): Input {
    return new MaterialInput(placeholder);
  }

  createModal(title: string): Modal {
    return new MaterialModal(title);
  }
}

/ Bootstrap Implementation
class BootstrapUIFactory implements UIFactory {
  createButton(label: string): Button {
    return new BootstrapButton(label);
  }

  createInput(placeholder: string): Input {
    return new BootstrapInput(placeholder);
  }

  createModal(title: string): Modal {
    return new BootstrapModal(title);
  }
}

/ Usage avec injection
class FormBuilder {
  constructor(private uiFactory: UIFactory) {}

  buildLoginForm(): string {
    const emailInput = this.uiFactory.createInput('Email');
    const passwordInput = this.uiFactory.createInput('Mot de passe');
    const submitButton = this.uiFactory.createButton('Se connecter');

    return `
      <form>
        ${emailInput.render()}
        ${passwordInput.render()}
        ${submitButton.render()}
      </form>
    `;
  }
}
```

### Builder Pattern

```typescript
/ types/query.types.ts
interface QueryConfig {
  readonly table: string;
  readonly columns: readonly string[];
  readonly conditions: readonly WhereCondition[];
  readonly joins: readonly JoinClause[];
  readonly orderBy: readonly OrderClause[];
  readonly limit?: number;
  readonly offset?: number;
}

interface WhereCondition {
  column: string;
  operator: '=' | '!=' | '>' | '<' | '>=' | '<=' | 'LIKE' | 'IN';
  value: unknown;
}

interface JoinClause {
  type: 'INNER' | 'LEFT' | 'RIGHT';
  table: string;
  on: string;
}

interface OrderClause {
  column: string;
  direction: 'ASC' | 'DESC';
}

/ ✅ Builder avec fluent interface
class QueryBuilder {
  private config: Partial<QueryConfig> = {
    columns: [],
    conditions: [],
    joins: [],
    orderBy: [],
  };

  from(table: string): this {
    this.config.table = table;
    return this;
  }

  select(...columns: string[]): this {
    this.config.columns = [...(this.config.columns ?? []), ...columns];
    return this;
  }

  where(column: string, operator: WhereCondition['operator'], value: unknown): this {
    this.config.conditions = [
      ...(this.config.conditions ?? []),
      { column, operator, value },
    ];
    return this;
  }

  whereEquals(column: string, value: unknown): this {
    return this.where(column, '=', value);
  }

  whereIn(column: string, values: unknown[]): this {
    return this.where(column, 'IN', values);
  }

  join(table: string, on: string, type: JoinClause['type'] = 'INNER'): this {
    this.config.joins = [
      ...(this.config.joins ?? []),
      { type, table, on },
    ];
    return this;
  }

  leftJoin(table: string, on: string): this {
    return this.join(table, on, 'LEFT');
  }

  orderBy(column: string, direction: OrderClause['direction'] = 'ASC'): this {
    this.config.orderBy = [
      ...(this.config.orderBy ?? []),
      { column, direction },
    ];
    return this;
  }

  limit(count: number): this {
    this.config.limit = count;
    return this;
  }

  offset(count: number): this {
    this.config.offset = count;
    return this;
  }

  build(): QueryConfig {
    if (!this.config.table) {
      throw new Error('Table is required');
    }

    if (this.config.columns?.length === 0) {
      this.config.columns = ['*'];
    }

    return this.config as QueryConfig;
  }

  toSQL(): string {
    const config = this.build();

    let sql = `SELECT ${config.columns.join(', ')} FROM ${config.table}`;

    / Joins
    for (const join of config.joins) {
      sql += ` ${join.type} JOIN ${join.table} ON ${join.on}`;
    }

    / Where
    if (config.conditions.length > 0) {
      const whereClauses = config.conditions.map(c => {
        if (c.operator === 'IN') {
          const values = (c.value as unknown[]).map(v => `'${v}'`).join(', ');
          return `${c.column} IN (${values})`;
        }
        return `${c.column} ${c.operator} '${c.value}'`;
      });
      sql += ` WHERE ${whereClauses.join(' AND ')}`;
    }

    / Order By
    if (config.orderBy.length > 0) {
      const orderClauses = config.orderBy.map(o => `${o.column} ${o.direction}`);
      sql += ` ORDER BY ${orderClauses.join(', ')}`;
    }

    / Limit & Offset
    if (config.limit !== undefined) {
      sql += ` LIMIT ${config.limit}`;
    }

    if (config.offset !== undefined) {
      sql += ` OFFSET ${config.offset}`;
    }

    return sql;
  }
}

/ Usage
const query = new QueryBuilder()
  .from('users')
  .select('id', 'name', 'email')
  .leftJoin('orders', 'orders.user_id = users.id')
  .whereEquals('status', 'active')
  .whereIn('role', ['admin', 'moderator'])
  .orderBy('created_at', 'DESC')
  .limit(10)
  .offset(20)
  .toSQL();

/ SELECT id, name, email FROM users LEFT JOIN orders ON orders.user_id = users.id WHERE status = 'active' AND role IN ('admin', 'moderator') ORDER BY created_at DESC LIMIT 10 OFFSET 20
```

## 🏗️ Structural Patterns

### Adapter Pattern

```typescript
/ interfaces/payment.interfaces.ts
interface PaymentProcessor {
  processPayment(amount: number, currency: string): Promise<PaymentResult>;
  refund(transactionId: string): Promise<RefundResult>;
}

interface PaymentResult {
  success: boolean;
  transactionId: string;
  message?: string;
}

interface RefundResult {
  success: boolean;
  refundId: string;
}

/ External Stripe SDK (simulé)
interface StripeChargeResponse {
  id: string;
  status: 'succeeded' | 'failed';
  amount: number;
  currency: string;
}

class StripeSDK {
  async createCharge(params: {
    amount: number;
    currency: string;
  }): Promise<StripeChargeResponse> {
    / Appel à l'API Stripe
    return {
      id: `ch_${Date.now()}`,
      status: 'succeeded',
      amount: params.amount,
      currency: params.currency,
    };
  }

  async createRefund(chargeId: string): Promise<{ id: string; status: string }> {
    return {
      id: `re_${Date.now()}`,
      status: 'succeeded',
    };
  }
}

/ ✅ Adapter pour Stripe
class StripePaymentAdapter implements PaymentProcessor {
  constructor(private stripe: StripeSDK) {}

  async processPayment(amount: number, currency: string): Promise<PaymentResult> {
    try {
      const charge = await this.stripe.createCharge({
        amount: Math.round(amount * 100), / Stripe utilise les centimes
        currency: currency.toLowerCase(),
      });

      return {
        success: charge.status === 'succeeded',
        transactionId: charge.id,
        message: charge.status === 'succeeded' ? 'Paiement réussi' : 'Paiement échoué',
      };
    } catch (error) {
      return {
        success: false,
        transactionId: '',
        message: `Erreur Stripe: ${(error as Error).message}`,
      };
    }
  }

  async refund(transactionId: string): Promise<RefundResult> {
    const refund = await this.stripe.createRefund(transactionId);
    return {
      success: refund.status === 'succeeded',
      refundId: refund.id,
    };
  }
}

/ ✅ Adapter pour PayPal
class PayPalPaymentAdapter implements PaymentProcessor {
  constructor(private paypal: PayPalSDK) {}

  async processPayment(amount: number, currency: string): Promise<PaymentResult> {
    const order = await this.paypal.createOrder({
      value: amount.toFixed(2),
      currencyCode: currency.toUpperCase(),
    });

    return {
      success: order.state === 'approved',
      transactionId: order.orderId,
    };
  }

  async refund(transactionId: string): Promise<RefundResult> {
    const refund = await this.paypal.refundCapture(transactionId);
    return {
      success: refund.status === 'COMPLETED',
      refundId: refund.id,
    };
  }
}

/ Service utilisant l'interface
class PaymentService {
  constructor(private processor: PaymentProcessor) {}

  async checkout(amount: number, currency: string = 'EUR'): Promise<PaymentResult> {
    / Validation, logging, etc.
    return this.processor.processPayment(amount, currency);
  }
}
```

### Decorator Pattern

```typescript
/ interfaces/logger.interfaces.ts
interface Logger {
  log(message: string): void;
  error(message: string, error?: Error): void;
  warn(message: string): void;
  debug(message: string): void;
}

/ Base implementation
class ConsoleLogger implements Logger {
  log(message: string): void {
    console.log(message);
  }

  error(message: string, error?: Error): void {
    console.error(message, error);
  }

  warn(message: string): void {
    console.warn(message);
  }

  debug(message: string): void {
    console.debug(message);
  }
}

/ ✅ Decorator de base
abstract class LoggerDecorator implements Logger {
  constructor(protected logger: Logger) {}

  log(message: string): void {
    this.logger.log(message);
  }

  error(message: string, error?: Error): void {
    this.logger.error(message, error);
  }

  warn(message: string): void {
    this.logger.warn(message);
  }

  debug(message: string): void {
    this.logger.debug(message);
  }
}

/ ✅ Decorator pour ajouter un timestamp
class TimestampLoggerDecorator extends LoggerDecorator {
  private formatMessage(message: string): string {
    const timestamp = new Date().toISOString();
    return `[${timestamp}] ${message}`;
  }

  override log(message: string): void {
    super.log(this.formatMessage(message));
  }

  override error(message: string, error?: Error): void {
    super.error(this.formatMessage(message), error);
  }

  override warn(message: string): void {
    super.warn(this.formatMessage(message));
  }

  override debug(message: string): void {
    super.debug(this.formatMessage(message));
  }
}

/ ✅ Decorator pour ajouter un contexte
class ContextLoggerDecorator extends LoggerDecorator {
  constructor(
    logger: Logger,
    private context: string,
  ) {
    super(logger);
  }

  private formatMessage(message: string): string {
    return `[${this.context}] ${message}`;
  }

  override log(message: string): void {
    super.log(this.formatMessage(message));
  }

  override error(message: string, error?: Error): void {
    super.error(this.formatMessage(message), error);
  }

  override warn(message: string): void {
    super.warn(this.formatMessage(message));
  }

  override debug(message: string): void {
    super.debug(this.formatMessage(message));
  }
}

/ ✅ Decorator pour filtrer par niveau
type LogLevel = 'debug' | 'log' | 'warn' | 'error';

class FilteredLoggerDecorator extends LoggerDecorator {
  private levels: Map<LogLevel, number> = new Map([
    ['debug', 0],
    ['log', 1],
    ['warn', 2],
    ['error', 3],
  ]);

  constructor(
    logger: Logger,
    private minLevel: LogLevel,
  ) {
    super(logger);
  }

  private shouldLog(level: LogLevel): boolean {
    const minLevelValue = this.levels.get(this.minLevel) ?? 0;
    const currentLevelValue = this.levels.get(level) ?? 0;
    return currentLevelValue >= minLevelValue;
  }

  override log(message: string): void {
    if (this.shouldLog('log')) {
      super.log(message);
    }
  }

  override error(message: string, error?: Error): void {
    if (this.shouldLog('error')) {
      super.error(message, error);
    }
  }

  override warn(message: string): void {
    if (this.shouldLog('warn')) {
      super.warn(message);
    }
  }

  override debug(message: string): void {
    if (this.shouldLog('debug')) {
      super.debug(message);
    }
  }
}

/ Usage - Composition de decorators
const logger: Logger = new FilteredLoggerDecorator(
  new TimestampLoggerDecorator(
    new ContextLoggerDecorator(
      new ConsoleLogger(),
      'UserService',
    ),
  ),
  'log', / Minimum level
);

logger.debug('Debug message'); / Filtré (niveau trop bas)
logger.log('Info message'); / [2024-01-15T10:30:00.000Z] [UserService] Info message
```

### Facade Pattern

```typescript
/ ✅ Facade pour un système de notifications complexe
interface NotificationOptions {
  userId: string;
  message: string;
  title?: string;
  data?: Record<string, unknown>;
}

/ Sous-systèmes complexes
class EmailService {
  async send(to: string, subject: string, body: string): Promise<boolean> {
    / Logique email complexe
    return true;
  }
}

class PushNotificationService {
  async send(deviceToken: string, title: string, body: string, data?: object): Promise<boolean> {
    / Logique push complexe
    return true;
  }
}

class SMSService {
  async send(phoneNumber: string, message: string): Promise<boolean> {
    / Logique SMS complexe
    return true;
  }
}

class UserPreferencesRepository {
  async getPreferences(userId: string): Promise<{
    email?: string;
    phone?: string;
    deviceToken?: string;
    emailEnabled: boolean;
    pushEnabled: boolean;
    smsEnabled: boolean;
  }> {
    / Récupérer les préférences utilisateur
    return {
      email: 'user@example.com',
      phone: '+33600000000',
      deviceToken: 'device-token-123',
      emailEnabled: true,
      pushEnabled: true,
      smsEnabled: false,
    };
  }
}

class NotificationLogRepository {
  async log(notification: {
    userId: string;
    type: string;
    message: string;
    success: boolean;
    timestamp: Date;
  }): Promise<void> {
    / Enregistrer le log
  }
}

/ ✅ Facade - Interface simplifiée
class NotificationFacade {
  constructor(
    private emailService: EmailService,
    private pushService: PushNotificationService,
    private smsService: SMSService,
    private preferencesRepo: UserPreferencesRepository,
    private logRepo: NotificationLogRepository,
  ) {}

  async notify(options: NotificationOptions): Promise<{
    email: boolean;
    push: boolean;
    sms: boolean;
  }> {
    const { userId, message, title = 'Notification', data } = options;

    / Récupérer les préférences
    const prefs = await this.preferencesRepo.getPreferences(userId);

    const results = {
      email: false,
      push: false,
      sms: false,
    };

    / Envoyer par email si activé
    if (prefs.emailEnabled && prefs.email) {
      results.email = await this.emailService.send(prefs.email, title, message);
      await this.logNotification(userId, 'email', message, results.email);
    }

    / Envoyer en push si activé
    if (prefs.pushEnabled && prefs.deviceToken) {
      results.push = await this.pushService.send(prefs.deviceToken, title, message, data);
      await this.logNotification(userId, 'push', message, results.push);
    }

    / Envoyer par SMS si activé
    if (prefs.smsEnabled && prefs.phone) {
      results.sms = await this.smsService.send(prefs.phone, message);
      await this.logNotification(userId, 'sms', message, results.sms);
    }

    return results;
  }

  private async logNotification(
    userId: string,
    type: string,
    message: string,
    success: boolean,
  ): Promise<void> {
    await this.logRepo.log({
      userId,
      type,
      message,
      success,
      timestamp: new Date(),
    });
  }
}

/ Usage simple
const notificationService = new NotificationFacade(
  new EmailService(),
  new PushNotificationService(),
  new SMSService(),
  new UserPreferencesRepository(),
  new NotificationLogRepository(),
);

await notificationService.notify({
  userId: 'user-123',
  message: 'Votre commande a été expédiée !',
  title: 'Expédition',
  data: { orderId: 'order-456' },
});
```

## 🎭 Behavioral Patterns

### Strategy Pattern

```typescript
/ interfaces/pricing.interfaces.ts
interface PricingStrategy {
  calculatePrice(basePrice: number, quantity: number): number;
  getName(): string;
}

/ ✅ Stratégies concrètes
class RegularPricingStrategy implements PricingStrategy {
  calculatePrice(basePrice: number, quantity: number): number {
    return basePrice * quantity;
  }

  getName(): string {
    return 'Regular';
  }
}

class BulkPricingStrategy implements PricingStrategy {
  constructor(
    private threshold: number = 10,
    private discountPercent: number = 15,
  ) {}

  calculatePrice(basePrice: number, quantity: number): number {
    const total = basePrice * quantity;
    if (quantity >= this.threshold) {
      return total * (1 - this.discountPercent / 100);
    }
    return total;
  }

  getName(): string {
    return 'Bulk';
  }
}

class SeasonalPricingStrategy implements PricingStrategy {
  constructor(
    private season: 'summer' | 'winter' | 'spring' | 'fall',
    private discounts: Record<string, number>,
  ) {}

  calculatePrice(basePrice: number, quantity: number): number {
    const discount = this.discounts[this.season] ?? 0;
    return basePrice * quantity * (1 - discount / 100);
  }

  getName(): string {
    return `Seasonal (${this.season})`;
  }
}

class LoyaltyPricingStrategy implements PricingStrategy {
  constructor(
    private loyaltyLevel: 'bronze' | 'silver' | 'gold' | 'platinum',
    private discounts: Record<string, number> = {
      bronze: 5,
      silver: 10,
      gold: 15,
      platinum: 20,
    },
  ) {}

  calculatePrice(basePrice: number, quantity: number): number {
    const discount = this.discounts[this.loyaltyLevel] ?? 0;
    return basePrice * quantity * (1 - discount / 100);
  }

  getName(): string {
    return `Loyalty (${this.loyaltyLevel})`;
  }
}

/ ✅ Contexte utilisant la stratégie
class ShoppingCart {
  private items: Array<{ productId: string; price: number; quantity: number }> = [];
  private strategy: PricingStrategy = new RegularPricingStrategy();

  setStrategy(strategy: PricingStrategy): void {
    this.strategy = strategy;
  }

  addItem(productId: string, price: number, quantity: number): void {
    this.items.push({ productId, price, quantity });
  }

  calculateTotal(): number {
    return this.items.reduce((total, item) => {
      return total + this.strategy.calculatePrice(item.price, item.quantity);
    }, 0);
  }

  getStrategyName(): string {
    return this.strategy.getName();
  }
}

/ Usage
const cart = new ShoppingCart();
cart.addItem('product-1', 100, 5);
cart.addItem('product-2', 50, 3);

/ Stratégie par défaut
console.log(cart.calculateTotal()); / 650

/ Changer de stratégie
cart.setStrategy(new BulkPricingStrategy(5, 10));
console.log(cart.calculateTotal()); / 585 (10% de réduction)

cart.setStrategy(new LoyaltyPricingStrategy('gold'));
console.log(cart.calculateTotal()); / 552.5 (15% de réduction)
```

### Observer Pattern

```typescript
/ types/event.types.ts
type EventCallback<T = unknown> = (data: T) => void;

interface Observer<T = unknown> {
  update(data: T): void;
}

interface Subject<T = unknown> {
  subscribe(observer: Observer<T>): () => void;
  unsubscribe(observer: Observer<T>): void;
  notify(data: T): void;
}

/ ✅ Implementation Type-Safe avec EventEmitter
class TypedEventEmitter<TEvents extends Record<string, unknown>> {
  private listeners = new Map<keyof TEvents, Set<EventCallback<unknown>>>();

  on<K extends keyof TEvents>(event: K, callback: EventCallback<TEvents[K]>): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }

    this.listeners.get(event)!.add(callback as EventCallback<unknown>);

    / Retourner une fonction de désinscription
    return () => this.off(event, callback);
  }

  off<K extends keyof TEvents>(event: K, callback: EventCallback<TEvents[K]>): void {
    this.listeners.get(event)?.delete(callback as EventCallback<unknown>);
  }

  emit<K extends keyof TEvents>(event: K, data: TEvents[K]): void {
    this.listeners.get(event)?.forEach(callback => {
      try {
        callback(data);
      } catch (error) {
        console.error(`Error in event listener for ${String(event)}:`, error);
      }
    });
  }

  once<K extends keyof TEvents>(event: K, callback: EventCallback<TEvents[K]>): () => void {
    const wrapper: EventCallback<TEvents[K]> = (data) => {
      this.off(event, wrapper);
      callback(data);
    };
    return this.on(event, wrapper);
  }

  removeAllListeners<K extends keyof TEvents>(event?: K): void {
    if (event) {
      this.listeners.delete(event);
    } else {
      this.listeners.clear();
    }
  }
}

/ ✅ Usage avec types d'événements définis
interface CartEvents {
  'item:added': { productId: string; quantity: number };
  'item:removed': { productId: string };
  'item:updated': { productId: string; quantity: number };
  'cart:cleared': void;
  'checkout:started': { total: number };
  'checkout:completed': { orderId: string; total: number };
}

class CartStore extends TypedEventEmitter<CartEvents> {
  private items = new Map<string, number>();

  addItem(productId: string, quantity: number = 1): void {
    const currentQty = this.items.get(productId) ?? 0;
    this.items.set(productId, currentQty + quantity);
    
    this.emit('item:added', { productId, quantity });
  }

  removeItem(productId: string): void {
    this.items.delete(productId);
    this.emit('item:removed', { productId });
  }

  updateQuantity(productId: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(productId);
    } else {
      this.items.set(productId, quantity);
      this.emit('item:updated', { productId, quantity });
    }
  }

  clear(): void {
    this.items.clear();
    this.emit('cart:cleared', undefined as unknown as void);
  }

  checkout(): void {
    const total = this.calculateTotal();
    this.emit('checkout:started', { total });
    
    / Simulation de checkout
    const orderId = `order-${Date.now()}`;
    this.emit('checkout:completed', { orderId, total });
    this.clear();
  }

  private calculateTotal(): number {
    / Simulation
    return Array.from(this.items.values()).reduce((sum, qty) => sum + qty * 10, 0);
  }
}

/ Usage
const cart = new CartStore();

/ Type-safe listeners
const unsubscribe = cart.on('item:added', ({ productId, quantity }) => {
  console.log(`Added ${quantity} of ${productId}`);
});

cart.on('checkout:completed', ({ orderId, total }) => {
  console.log(`Order ${orderId} completed: ${total}€`);
});

cart.addItem('product-1', 2);
cart.addItem('product-2', 1);
cart.checkout();

/ Cleanup
unsubscribe();
```

### Command Pattern

```typescript
/ interfaces/command.interfaces.ts
interface Command {
  execute(): Promise<void>;
  undo(): Promise<void>;
  canUndo(): boolean;
}

/ ✅ Commandes concrètes
class AddTodoCommand implements Command {
  private addedTodo: Todo | null = null;

  constructor(
    private store: TodoStore,
    private todo: Omit<Todo, 'id' | 'createdAt'>,
  ) {}

  async execute(): Promise<void> {
    this.addedTodo = await this.store.add(this.todo);
  }

  async undo(): Promise<void> {
    if (this.addedTodo) {
      await this.store.remove(this.addedTodo.id);
    }
  }

  canUndo(): boolean {
    return this.addedTodo !== null;
  }
}

class RemoveTodoCommand implements Command {
  private removedTodo: Todo | null = null;

  constructor(
    private store: TodoStore,
    private todoId: string,
  ) {}

  async execute(): Promise<void> {
    this.removedTodo = await this.store.getById(this.todoId);
    await this.store.remove(this.todoId);
  }

  async undo(): Promise<void> {
    if (this.removedTodo) {
      await this.store.restore(this.removedTodo);
    }
  }

  canUndo(): boolean {
    return this.removedTodo !== null;
  }
}

class ToggleTodoCommand implements Command {
  private previousState: boolean | null = null;

  constructor(
    private store: TodoStore,
    private todoId: string,
  ) {}

  async execute(): Promise<void> {
    const todo = await this.store.getById(this.todoId);
    this.previousState = todo?.completed ?? null;
    await this.store.toggle(this.todoId);
  }

  async undo(): Promise<void> {
    if (this.previousState !== null) {
      await this.store.setCompleted(this.todoId, this.previousState);
    }
  }

  canUndo(): boolean {
    return this.previousState !== null;
  }
}

/ ✅ Command Manager avec historique
class CommandManager {
  private history: Command[] = [];
  private undoneCommands: Command[] = [];
  private maxHistory: number;

  constructor(options: { maxHistory?: number } = {}) {
    this.maxHistory = options.maxHistory ?? 50;
  }

  async execute(command: Command): Promise<void> {
    await command.execute();
    
    this.history.push(command);
    this.undoneCommands = []; / Clear redo stack
    
    / Limiter l'historique
    if (this.history.length > this.maxHistory) {
      this.history.shift();
    }
  }

  async undo(): Promise<boolean> {
    const command = this.history.pop();
    
    if (!command || !command.canUndo()) {
      return false;
    }

    await command.undo();
    this.undoneCommands.push(command);
    
    return true;
  }

  async redo(): Promise<boolean> {
    const command = this.undoneCommands.pop();
    
    if (!command) {
      return false;
    }

    await command.execute();
    this.history.push(command);
    
    return true;
  }

  canUndo(): boolean {
    return this.history.length > 0 && this.history[this.history.length - 1].canUndo();
  }

  canRedo(): boolean {
    return this.undoneCommands.length > 0;
  }

  clear(): void {
    this.history = [];
    this.undoneCommands = [];
  }
}

/ Usage
const todoStore = new TodoStore();
const commandManager = new CommandManager();

/ Exécuter des commandes
await commandManager.execute(new AddTodoCommand(todoStore, { title: 'Task 1' }));
await commandManager.execute(new AddTodoCommand(todoStore, { title: 'Task 2' }));
await commandManager.execute(new ToggleTodoCommand(todoStore, 'todo-1'));

/ Undo/Redo
await commandManager.undo(); / Annule le toggle
await commandManager.undo(); / Annule l'ajout de Task 2
await commandManager.redo(); / Refait l'ajout de Task 2
```

## 🔌 Repository Pattern

```typescript
/ interfaces/repository.interfaces.ts
interface Entity {
  id: string;
}

interface Repository<T extends Entity> {
  findById(id: string): Promise<T | null>;
  findAll(options?: FindOptions<T>): Promise<T[]>;
  create(entity: Omit<T, 'id'>): Promise<T>;
  update(id: string, updates: Partial<T>): Promise<T>;
  delete(id: string): Promise<void>;
  count(filter?: Partial<T>): Promise<number>;
}

interface FindOptions<T> {
  filter?: Partial<T>;
  sort?: { field: keyof T; order: 'asc' | 'desc' };
  pagination?: { page: number; limit: number };
}

/ ✅ Implementation générique
class BaseRepository<T extends Entity> implements Repository<T> {
  constructor(protected collection: Map<string, T> = new Map()) {}

  async findById(id: string): Promise<T | null> {
    return this.collection.get(id) ?? null;
  }

  async findAll(options?: FindOptions<T>): Promise<T[]> {
    let items = Array.from(this.collection.values());

    / Filtrage
    if (options?.filter) {
      items = items.filter(item => {
        return Object.entries(options.filter!).every(([key, value]) => {
          return item[key as keyof T] === value;
        });
      });
    }

    / Tri
    if (options?.sort) {
      const { field, order } = options.sort;
      items.sort((a, b) => {
        const aVal = a[field];
        const bVal = b[field];
        const comparison = aVal < bVal ? -1 : aVal > bVal ? 1 : 0;
        return order === 'asc' ? comparison : -comparison;
      });
    }

    / Pagination
    if (options?.pagination) {
      const { page, limit } = options.pagination;
      const start = (page - 1) * limit;
      items = items.slice(start, start + limit);
    }

    return items;
  }

  async create(entity: Omit<T, 'id'>): Promise<T> {
    const id = this.generateId();
    const newEntity = { ...entity, id } as T;
    this.collection.set(id, newEntity);
    return newEntity;
  }

  async update(id: string, updates: Partial<T>): Promise<T> {
    const existing = await this.findById(id);
    if (!existing) {
      throw new Error(`Entity with id ${id} not found`);
    }
    
    const updated = { ...existing, ...updates, id };
    this.collection.set(id, updated);
    return updated;
  }

  async delete(id: string): Promise<void> {
    const exists = this.collection.has(id);
    if (!exists) {
      throw new Error(`Entity with id ${id} not found`);
    }
    this.collection.delete(id);
  }

  async count(filter?: Partial<T>): Promise<number> {
    const items = await this.findAll({ filter });
    return items.length;
  }

  protected generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}

/ ✅ Repository spécifique avec méthodes métier
interface User extends Entity {
  id: string;
  email: string;
  name: string;
  role: 'admin' | 'user' | 'moderator';
  active: boolean;
  createdAt: Date;
}

class UserRepository extends BaseRepository<User> {
  async findByEmail(email: string): Promise<User | null> {
    const users = await this.findAll({ filter: { email } as Partial<User> });
    return users[0] ?? null;
  }

  async findActiveUsers(): Promise<User[]> {
    return this.findAll({ filter: { active: true } as Partial<User> });
  }

  async findByRole(role: User['role']): Promise<User[]> {
    return this.findAll({ filter: { role } as Partial<User> });
  }

  async deactivate(id: string): Promise<User> {
    return this.update(id, { active: false });
  }

  async countByRole(role: User['role']): Promise<number> {
    return this.count({ role } as Partial<User>);
  }
}

/ Usage
const userRepo = new UserRepository();

const newUser = await userRepo.create({
  email: 'john@example.com',
  name: 'John Doe',
  role: 'user',
  active: true,
  createdAt: new Date(),
});

const admins = await userRepo.findByRole('admin');
const activeCount = await userRepo.count({ active: true } as Partial<User>);
```

## 🎯 Dependency Injection

```typescript
/ ✅ Container DI simple
type Constructor<T = unknown> = new (...args: unknown[]) => T;
type Factory<T = unknown> = () => T;

class Container {
  private instances = new Map<string, unknown>();
  private factories = new Map<string, Factory>();
  private singletons = new Set<string>();

  register<T>(token: string, factory: Factory<T>, singleton = false): this {
    this.factories.set(token, factory);
    if (singleton) {
      this.singletons.add(token);
    }
    return this;
  }

  registerClass<T>(token: string, ctor: Constructor<T>, singleton = false): this {
    return this.register(token, () => new ctor(), singleton);
  }

  registerInstance<T>(token: string, instance: T): this {
    this.instances.set(token, instance);
    this.singletons.add(token);
    return this;
  }

  resolve<T>(token: string): T {
    / Vérifier si instance existe déjà (singleton)
    if (this.instances.has(token)) {
      return this.instances.get(token) as T;
    }

    / Créer l'instance
    const factory = this.factories.get(token);
    if (!factory) {
      throw new Error(`No factory registered for token: ${token}`);
    }

    const instance = factory() as T;

    / Stocker si singleton
    if (this.singletons.has(token)) {
      this.instances.set(token, instance);
    }

    return instance;
  }
}

/ ✅ Usage avec interfaces
interface ILogger {
  log(message: string): void;
}

interface IUserRepository {
  findById(id: string): Promise<User | null>;
}

interface IUserService {
  getUser(id: string): Promise<User | null>;
}

/ Implémentations
class ConsoleLogger implements ILogger {
  log(message: string): void {
    console.log(`[LOG] ${message}`);
  }
}

class InMemoryUserRepository implements IUserRepository {
  private users = new Map<string, User>();

  async findById(id: string): Promise<User | null> {
    return this.users.get(id) ?? null;
  }
}

class UserService implements IUserService {
  constructor(
    private logger: ILogger,
    private userRepo: IUserRepository,
  ) {}

  async getUser(id: string): Promise<User | null> {
    this.logger.log(`Getting user ${id}`);
    return this.userRepo.findById(id);
  }
}

/ Configuration du container
const container = new Container();

container
  .registerClass<ILogger>('ILogger', ConsoleLogger, true)
  .registerClass<IUserRepository>('IUserRepository', InMemoryUserRepository, true)
  .register<IUserService>(
    'IUserService',
    () =>
      new UserService(
        container.resolve<ILogger>('ILogger'),
        container.resolve<IUserRepository>('IUserRepository'),
      ),
    true,
  );

/ Résolution
const userService = container.resolve<IUserService>('IUserService');
```
