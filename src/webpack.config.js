module.exports = {
    resolve: {
        fallback: {
            // buffer: require.resolve("buffer/"),
            stream: require.resolve("stream-browserify"),
            // crypto: require.resolve("crypto-browserify"),
        },
    },
};
