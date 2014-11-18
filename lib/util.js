var _ = require('lodash'),
    bunyan = require('bunyan'),
    config = require('../config/config'),
    logger, logConfig;

logConfig = {
  name: config.name,
  streams: [
    {
      level: 'warn',
      stream: process.stderr
    },
    {
      level: 'debug',
      stream: process.stderr
    }
  ]
};

module.exports = {
  createPath: function joinPath(var_args) {
    var args = Array.prototype.slice.call(arguments);
    args.unshift(config.api.version);
    return args.join('/');
  },

  logger: function() {
    if (!logger) {
      logger = bunyan.createLogger({name: config.name});
      if (process.env.NODE_MARKETO_LOG_LEVEL) {
        logger.level(bunyan.resolveLevel(process.env.NODE_MARKETO_LOG_LEVEL));
      }
    }
    return logger;
  },

  formatOptions: function(options, opt_filterArray) {
    options = _.clone(options);

    if (opt_filterArray) {
      options = _.pick(options, opt_filterArray);
    }

    _.each(['fields', 'filterValues'], function(key) {
      if (_.isArray(options[key])) {
        options[key] = options[key].join(',');
      }
    });

    return options;
  }
};
