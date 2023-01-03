import { Component, OnInit } from '@angular/core';
import { WebProvider } from '@blockcore/provider';
import * as bitcoinMessage from 'bitcoinjs-message';
const { v4: uuidv4 } = require('uuid');
import { DIDResolutionOptions, Resolver } from 'did-resolver';
import is from '@blockcore/did-resolver';
import * as secp256k1 from '@noble/secp256k1';
import { sha256 } from '@noble/hashes/sha256';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  title = 'dapp-sample';
  provider?: WebProvider;

  signingText: string = 'Hello World';
  signingJson: string = '{ "id": 5, "text": "Hello World" }';

  signedTextSignature?: string;
  signedTextKey?: string;
  signedTextNetwork?: string;
  signedTextValidSignature?: boolean;

  signedJsonSignature?: string;
  signedJsonKey?: string;
  signedJsonNetwork?: string;
  signedJsonValidSignature?: boolean;

  paymentRequestAmount = 2;
  paymentVerificationRequestAmount = 5;
  paymentVerificationTransactionId: string | undefined = undefined;
  paymentTransactionId: string | undefined = undefined;

  didSupportedMethodsResponse?: string[];
  didRequestResponse: any;

  wallet: any;
  nostrPublicKey = '';
  nostrSignedEvent = '';
  nostrRelays?: string[];

  nostrEvent = {
    created_at: Date.now(),
    kind: 1,
    tags: [],
    content: 'This is my nostr message',
    pubkey: '',
  };

  // vcSubject = 'did:is:';
  vcType = 'EmailVerification';
  vcID = uuidv4();
  vcClaim = '{ "id": "did:is:0f254e55a2633d468e92aa7dd5a76c0c9101fab8e282c8c20b3fefde0d68f217", "sameAs": "mail@mail.com" }';
  vc: string | undefined | null = null;

  networks = [
    { name: 'Any (user selected)', id: '' },
    { name: 'Bitcoin', id: 'BTC' },
    { name: 'City Chain', id: 'CITY' },
    { name: 'Stratis', id: 'STRAX' },
    { name: 'x42', id: 'X42' },
  ];

  network = 'BTC';

  constructor() {}

  ngOnInit(): void {
    console.log('This will be false:', this.isBlockcoreInstalled());

    setTimeout(() => {
      console.log('This will hopefully be true:', this.isBlockcoreInstalled());
    }, 250);
  }

  isBlockcoreInstalled = () => {
    const { blockcore } = globalThis as any;
    return Boolean(blockcore);
  };

  async initialize() {
    // Creating the WebProvider will perform multiple network requests to
    // get all known blockchain APIs.
    this.provider = await WebProvider.Create();
    this.provider.setNetwork(this.network);

    this.vcID = uuidv4();
  }

  async getNostrPublicKey() {
    const gt = globalThis as any;

    // Use nostr directly on global, similar to how most Nostr app will interact with the provider.
    const pubKey = await gt.nostr.getPublicKey();

    this.nostrPublicKey = pubKey;

    this.nostrEvent.pubkey = this.nostrPublicKey;
  }

  serializeEvent(evt: any): string {
    return JSON.stringify([
      0,
      evt.pubkey,
      evt.created_at,
      evt.kind,
      evt.tags,
      evt.content
    ])
  }

  getEventHash(event: Event): string {
    const utf8Encoder = new TextEncoder();
    let eventHash = sha256(utf8Encoder.encode(this.serializeEvent(event)));
    return secp256k1.utils.bytesToHex(eventHash);
  }

  async nostrSignEvent(event: any) {
    const gt = globalThis as any;

    event.id = this.getEventHash(event);

    try {
      // Use nostr directly on global, similar to how most Nostr app will interact with the provider.
      const signedEvent = await gt.nostr.signEvent(event);
      this.nostrSignedEvent = signedEvent;
    } catch (err: any) {
      console.error(err);
      this.nostrSignedEvent = err.toString();
    }
  }

  async getNostrPublicRelays() {
    const gt = globalThis as any;
    const relays = await gt.nostr.getRelays();
    this.nostrRelays = relays;
  }

  async nostrEncrypt() {
    this.nostrCipher = null;

    const gt = globalThis as any;
    const cipher = await gt.nostr.nip04.encrypt(this.nostrPublicKey, this.nostrEvent.content);
    this.nostrCipher = cipher;
  }

  nostrCipher = null;
  nostrDecrypted = null;

  async nostrDecrypt() {
    this.nostrDecrypted = null;

    const gt = globalThis as any;
    const content = await gt.nostr.nip04.decrypt(this.nostrPublicKey, this.nostrCipher);
    this.nostrDecrypted = content;
  }

  async signMessageAnyAccount(value: string) {
    const result: any = await this.provider!.request({
      method: 'signMessage',
      params: [{ message: value, network: this.provider?.indexer.network }],
    });
    console.log('Result:', result);

    this.signedTextKey = result.key;
    this.signedTextSignature = result.response.signature;
    this.signedTextNetwork = result.network;
    this.signedTextValidSignature = bitcoinMessage.verify(value, result.key, result.response.signature);
  }

  async signMessageAnyAccountJson(value: string) {
    const message = JSON.parse(value);

    const result: any = await this.provider!.request({
      method: 'signMessage',
      params: [{ message: message, network: this.provider?.indexer.network }],
    });

    console.log('Result:', result);

    this.signedJsonKey = result.key;
    this.signedJsonSignature = result.response.signature;
    this.signedJsonNetwork = result.network;
    const preparedMessage = JSON.stringify(message);
    this.signedJsonValidSignature = bitcoinMessage.verify(preparedMessage, result.key, result.response.signature);
  }

  async connect() {
    const challenge = uuidv4();

    try {
      var result: any = await this.provider!.request({
        method: 'wallets',
        params: [
          {
            challenge: challenge,
            // reason: 'Sample app want you to sign a verifiable credential with any of your DIDs.',
          },
        ],
      });

      console.log('Result:', result);
      this.wallet = result;
    } catch (err) {
      console.error(err);
    }
  }

  getAccounts() {}

  async paymentVerificationRequest(network: string, amount: number) {
    const paymentId = uuidv4();

    try {
      var result: any = await this.provider!.request({
        method: 'payment',
        params: [
          {
            network: network.toLowerCase(),
            amount: amount,
            address: 'CRp1q2hdFN5e1hVEEkFY3egyD2cT9ed6S3',
            label: 'City Chain Registry',
            message: 'Please make the initial payment for crypto company registration',
            id: paymentId,
          },
        ],
      });

      console.log('PAYMENT VERIFICATION REQUEST CLOSED...');
      console.log('Result:', result);

      this.paymentVerificationTransactionId = result.transactionId;
    } catch (err) {
      console.error(err);
    }
  }

  async paymentRequest(network: string, amount: number) {
    try {
      var result: any = await this.provider!.request({
        method: 'payment',
        params: [
          {
            network: network.toLowerCase(),
            amount: amount,
            address: 'Ccoquhaae7u6ASqQ5BiYueASz8EavUXrKn',
            label: 'Your Local Info',
            message: 'Invoice Number 5',
            data: 'MzExMzUzNDIzNDY',
            id: '4324',
          },
        ],
      });

      console.log('Result:', result);
      this.paymentTransactionId = result.transactionId;
    } catch (err) {
      console.error(err);
    }
  }

  async paymentRequestCity() {
    try {
      var result = await this.provider!.request({
        method: 'payment',
        params: [
          {
            network: 'city',
            amount: 10.5,
            address: 'Ccoquhaae7u6ASqQ5BiYueASz8EavUXrKn',
            label: 'Your Local Info',
            message: 'Invoice Number 5',
            data: 'MzExMzUzNDIzNDY',
            id: '4324',
          },
        ],
      });

      console.log('Result:', result);
    } catch (err) {
      console.error(err);
    }
  }

  async request(method: string, params?: object | unknown[]) {
    if (!params) {
      params = [];
    }

    const result: any = await this.provider!.request({
      method: method,
      params: params,
    });
    console.log('Result:', result);

    return result;
  }

  async didSupportedMethods() {
    const result = await this.request('did.supportedMethods');
    this.didSupportedMethodsResponse = result.response;
  }

  async didRequest(methods: string[]) {
    const result = await this.request('did.request', [
      {
        challenge: 'fc0949c4-fd9c-4825-b18d-79348e358156',
        methods: methods,
        reason: 'Sample app need access to any of your DIDs.',
      },
    ]);

    this.didRequestResponse = result;
  }

  vcRequestResponse: any;
  didLookup = 'did:is:0f254e55a2633d468e92aa7dd5a76c0c9101fab8e282c8c20b3fefde0d68f217';
  didLookupResponse: any | undefined;

  async resolveDid() {
    const isResolver = is.getResolver();
    const resolver = new Resolver(isResolver);
    this.didLookupResponse = await resolver.resolve(this.didLookup, {});
  }

  async vcRequest() {
    const result = await this.request('vc.request', [
      {
        challenge: 'fc0949c4-fd9c-4825-b18d-79348e358156',
        type: this.vcType,
        id: this.vcID,
        claim: this.vcClaim,
        reason: 'Sample app want you to sign a verifiable credential with any of your DIDs.',
      },
    ]);

    this.vcRequestResponse = result;
  }

  onNetworkChanged() {
    console.log(this.network);
    this.provider?.setNetwork(this.network);
  }

  async paymentRequestStrax() {
    try {
      var result = await this.provider!.request({
        method: 'payment',
        params: [
          {
            network: 'strax',
            amount: 2,
            address: 'Xcoquhaae7u6ASqQ5BiYueASz8EavUXrKn',
            label: 'Your Local Info',
            message: 'Invoice Number 5',
            data: 'MzExMzUzNDIzNDY',
            id: '4324',
          },
        ],
      });

      console.log('Result:', result);
    } catch (err) {
      console.error(err);
    }
  }
}
