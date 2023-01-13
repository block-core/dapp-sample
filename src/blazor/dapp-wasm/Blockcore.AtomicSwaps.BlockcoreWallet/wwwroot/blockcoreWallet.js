export async function hasBlockcoreWallet() {
    return (globalThis.blockcore != undefined);
}

export async function signMessageAnyAccount(value) {
    const provider = globalThis.blockcore;

    const result = await provider.request({
        method: 'signMessage',
        params: [{ message: value }],
    });
    console.log('Result:', result);
    return JSON.stringify(result);

    //var key = result.key;
    //var signature = result.signature;
    //var network = result.network;
    //var verify = bitcoinMessage.verify(value, result.key, result.signature);
}

export async function sendCoins(input) {
    const provider = globalThis.blockcore;

    var data = JSON.parse(input)
    const result = await provider.request({
        method: 'transaction.send',
        params: [data],
    });
    console.log('Result:', result);
    return JSON.stringify(result);
}

export async function swapCoins(input) {
    const provider = globalThis.blockcore;

    var data = JSON.parse(input)
    const result = await provider.request({
        method: 'atomicswaps.send',
        params: [data],
    });
    console.log('Result:', result);
    return JSON.stringify(result.response);
}

export async function getWallet(key) {
    const provider = globalThis.blockcore;

    const result = await provider.request({
        method: 'wallets',
        params: [{ key: key }],
    });
    console.log('Result:', result);
    return JSON.stringify(result);
}

export async function getSwapKey(key, walletId, accountId, includePrivateKey) {
    const provider = globalThis.blockcore;

    const result = await provider.request({
        method: 'atomicswaps.key',
        params: [{ key: key, walletId: walletId, accountId: accountId, includePrivateKey: includePrivateKey }],
    });
    console.log('Result:', result);
    return JSON.stringify(result);
}

export async function getSwapSecret(key, walletId, accountId, message) {
    const provider = globalThis.blockcore;

    const result = await provider.request({
        method: 'atomicswaps.secret',
        params: [{ key: key, walletId: walletId, accountId: accountId, message: message }],
    });
    console.log('Result:', result);
    return JSON.stringify(result);
}

export async function signMessageAnyAccountJson(value) {
    const message = JSON.parse(value);

    const provider = globalThis.blockcore;

    const result = await provider.request({
        method: 'signMessage',
        params: [{ message: message }],
    });

    console.log('Result:', result);
    return JSON.stringify(result);

    //this.signedJsonKey = result.key;
    //this.signedJsonSignature = result.signature;
    //this.signedJsonNetwork = result.network;
    //const preparedMessage = JSON.stringify(message);
    //this.signedJsonValidSignature = bitcoinMessage.verify(preparedMessage, result.key, result.signature);
}

export async function paymentRequest(network, amount) {
    try {
        const provider = globalThis.blockcore;

        var result = await provider.request({
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
        return JSON.stringify(result);
    } catch (err) {
        console.error(err);
    }
}

async function request(method, params) {
    if (!params) {
        params = [];
    }
    const provider = globalThis.blockcore;
    const result = await provider.request({
        method: method,
        params: params,
    });
    console.log('Result:', result);

    return result;
}

export async function didSupportedMethods() {
    const result = await request('did.supportedMethods');
    return JSON.stringify(result.response);
}

export async function didRequest(methods) {
    const result = await request('did.request', [
        {
            challenge: 'fc0949c4-fd9c-4825-b18d-79348e358156',
            methods: methods,
            reason: 'Sample app need access to any of your DIDs.',
        },
    ]);

    return JSON.stringify(result.response);
}

export async function signMessage(msg) {
    const provider = globalThis.blockcore;
    let signature;
    try {
        signature = await provider.request({ method: "signMessage", params: [{ scheme: "schnorr", message: msg }] });
        return JSON.stringify(signature);
    }
    catch (error) {
        console.error("Error: " + error.message);
        // User denied account access...
        throw "UserDenied";
    }
}





