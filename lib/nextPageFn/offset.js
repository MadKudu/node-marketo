var _ = require('lodash');

module.exports =  (conn, method, args, options) => {
    return () => {
        var params = options;
        if (method === 'get') {
            params = options.query = options.query || {};
        } else if (method === 'post' || method === 'put') {
            params = options.data = options.data || {};
        }

        params.offset = (params.offset || 0) + params.maxReturn;
        
        return conn._request.apply(conn, _.flatten([method, args, options], true));
    }
};
