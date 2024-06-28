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

  // rawPSBT is a Base64-encoded string representing the PSBT
  rawPSBT: string =
    '70736274ff01009a020000000258e87a21b56daf0c23be8e7070456c336f7cbaa5c8757924f545887bb2abdd750000000000ffffffff838d0427d0ec650a68aa46bb0b098aea4422c071b2ca78352a077959d07cea1d0100000000ffffffff0270aaf00800000000160014d85c2b71d0060b09c9886aeb815e50991dda124d00e1f5050000000016001400aea9a2e5f0f876a588df5546e8742d1d87008f00000000000100bb0200000001aad73931018bd25f84ae400b68848be09db706eac2ac18298babee71ab656f8b0000000048473044022058f6fc7c6a33e1b31548d481c826c015bd30135aad42cd67790dab66d2ad243b02204a1ced2604c6735b6393e5b41691dd78b00f0c5942fb9f751856faa938157dba01feffffff0280f0fa020000000017a9140fb9463421696b82c833af241c78c17ddbde493487d0f20a270100000017a91429ca74f8a08f81999428185c97b5d852e4063f618765000000010304010000000104475221029583bf39ae0a609747ad199addd634fa6108559d6c5cd39b4c2183f1ab96e07f2102dab61ff49a14db6a7d02b0cd1fbb78fc4b18312b5b4e54dae4dba2fbfef536d752ae2206029583bf39ae0a609747ad199addd634fa6108559d6c5cd39b4c2183f1ab96e07f10d90c6a4f000000800000008000000080220602dab61ff49a14db6a7d02b0cd1fbb78fc4b18312b5b4e54dae4dba2fbfef536d710d90c6a4f0000008000000080010000800001012000c2eb0b0000000017a914b7f5faf40e3d40a5a459b1db3535f2b72fa921e8870103040100000001042200208c2353173743b595dfb4a07b72ba8e42e3797da74e87fe7d9d7497e3b2028903010547522103089dc10c7ac6db54f91329af617333db388cead0c231f723379d1b99030b02dc21023add904f3d6dcf59ddb906b0dee23529b7ffb9ed50e5e86151926860221f0e7352ae2206023add904f3d6dcf59ddb906b0dee23529b7ffb9ed50e5e86151926860221f0e7310d90c6a4f000000800000008003000080220603089dc10c7ac6db54f91329af617333db388cead0c231f723379d1b99030b02dc10d90c6a4f00000080000000800200008000220203a9a4c37f5996d3aa25dbac6b570af0650394492942460b354753ed9eeca5877110d90c6a4f000000800000008004000080002202027f6399757d2eff55a136ad02c684b1838b6556e5f1b6b34282a94b6b5005109610d90c6a4f00000080000000800500008000';

  signedPsbtSignature?: string;

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
    return JSON.stringify([0, evt.pubkey, evt.created_at, evt.kind, evt.tags, evt.content]);
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

  transactionResult: any = undefined;

  async sendTransaction() {
    const result: any = await this.provider!.request({
      method: 'transaction.send',
      params: [
        {
          recipients: [
            { hint: 'Swap', address: 'CMrc2rFsPd9VnxPiHvN2wHFVNfNd9vY8Ze', amount: 100000000 },
            { hint: 'Fee Service', address: 'CMrc2rFsPd9VnxPiHvN2wHFVNfNd9vY8Ze', amount: 20000000 },
          ],
          // data: 'op_return',
          feeRate: 'medium',
          network: this.provider?.indexer.network,
        },
      ],
    });

    console.log('Result:', result);

    this.transactionResult = result;

    // this.signedTextKey = result.key;
    // this.signedTextSignature = result.response.signature;
    // this.signedTextNetwork = result.network;
    // this.signedTextValidSignature = bitcoinMessage.verify(value, result.key, result.response.signature);
  }

  async atomicSwapKey() {
    const result: any = await this.provider!.request({
      method: 'atomicswaps.key',
      params: [
        {
          walletId: this.wallet.response.wallet.id,
          accountId: this.atomicSwapAccountId,
          network: this.provider?.indexer.network,
        },
      ],
    });

    console.log('Result:', result);
    this.atomicSwapPublicKey = result.response.publicKey;
  }

  atomicSwapPublicKey?: string;
  atomicSwapSecretKey?: string;

  async atomicSwapSecret() {
    const result: any = await this.provider!.request({
      method: 'atomicswaps.secret',
      params: [
        {
          walletId: this.wallet.response.wallet.id,
          accountId: this.atomicSwapAccountId,
          network: this.provider?.indexer.network,
          message: '1', // "sessionid"?
        },
      ],
    });

    console.log('Result:', result);
    this.atomicSwapSecretKey = result.response.secret;
  }

  async atomicSwapSend() {
    const result: any = await this.provider!.request({
      method: 'atomicswaps.send',
      params: [
        {
          walletId: this.wallet.response.wallet.id,
          accountId: this.atomicSwapAccountId,
          network: this.provider?.indexer.network,
        },
      ],
    });

    console.log('Result:', result);
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

  async signPsbtAnyAccount(value: string) {
    const result: any = await this.provider!.request({
      method: 'signPsbt',
      params: [{ message: value, network: this.provider?.indexer.network }],
    });
    console.log('Result:', result);

    this.signedPsbtSignature = result.response.signature;
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

  atomicSwapAccountId?: string;

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
