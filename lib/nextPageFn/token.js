var _ = require('lodash');

module.exports =  (conn, method, args, options) => {
    return (nextPageToken) => {
        var params = options;
        if (method === 'get') {
            params = options.query = options.query || {};
        } else if (method === 'post' || method === 'put') {
            params = options.data = options.data || {};
        }

        params.nextPageToken = nextPageToken;

        return conn._request.apply(conn, _.flatten([method, args, options], true));
    }
};
