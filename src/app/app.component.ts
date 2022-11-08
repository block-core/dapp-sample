import { Component, OnInit } from '@angular/core';
import { WebProvider } from '@blockcore/provider';
import * as bitcoinMessage from 'bitcoinjs-message';

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
  }

  setNetwork(network: string) {
    this.provider?.setNetwork(network);
    console.log(this.provider);
  }

  async signMessageAnyAccount(value: string) {
    const result: any = await this.provider!.request({
      method: 'signMessage',
      params: [{ message: value }],
    });
    console.log('Signing result:', result);

    this.signedTextKey = result.key;
    this.signedTextSignature = result.signature;
    this.signedTextValidSignature = bitcoinMessage.verify(value, result.key, result.signature);
  }

  async signMessageAnyAccountJson(value: string) {
    const message = JSON.parse(value);

    const result: any = await this.provider!.request({
      method: 'signMessage',
      params: [{ message: message }],
    });

    console.log('Signing result:', result);

    this.signedJsonKey = result.key;
    this.signedJsonSignature = result.signature;
    const preparedMessage = JSON.stringify(message);
    this.signedJsonValidSignature = bitcoinMessage.verify(preparedMessage, result.key, result.signature);
  }

  async signMessage(network?: string) {
    if (!network) {
      network = this.provider?.indexer.network;
    }

    console.log('NETWORK:', network);

    const signing1 = await this.provider!.request({
      method: 'signMessage',
      params: [{ message: 'Hello World!' }],
    });

    console.log('Signing1:', signing1);
  }

  connect() {
    if (this.isBlockcoreInstalled()) {
      alert('ues!');
    } else {
    }
  }

  getAccounts() {}

  async paymentRequest(amount: number) {
    try {
      var result = await this.provider!.request({
        method: 'payment',
        params: [
          {
            network: 'city',
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
