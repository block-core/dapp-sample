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

  async signMessageAnyAccount() {
    const message = 'Hello World';

    const result: any = await this.provider!.request({
      method: 'signMessage',
      params: [{ message: message }],
    });

    console.log('Signing result:', result);
    console.log('Message:', message);
    console.log('Key:', result.key);
    console.log('Signature:', result.signature);

    var valid = bitcoinMessage.verify(message, result.key, result.signature);

    console.log('Signature is valid?', valid);
  }

  async signMessageAnyAccountJson() {
    const message = { id: 5, text: 'Hello World' };

    const result: any = await this.provider!.request({
      method: 'signMessage',
      params: [{ message: message }],
    });

    console.log('Signing result:', result);

    const preparedMessage = JSON.stringify(message);
    var valid = bitcoinMessage.verify(preparedMessage, result.key, result.signature);

    console.log('Signature is valid?', valid);
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
