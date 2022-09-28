const { Readable } = require('stream');

const Retry = require('./retry');

const DEFAULT_RANGE_SIZE = 750000; // 750 KB

class BulkFileStream extends Readable {
  constructor(_connection, path, fileSize, rangeSize) {
    super();
    this._connection = _connection;
    this.path = path;
    this.start = 0;
    this.fileSize = fileSize;
    this.rangeSize = rangeSize || DEFAULT_RANGE_SIZE;

    this._retry = new Retry();
  }

  async _read() {
    if (this.start !== null) {
      let end = this.start + this.rangeSize;
      if (end >= this.fileSize) {
        end = this.fileSize - 1;
      }

      try {
        const self = this;
        const requestFn = function () {
          return new Promise(async function (resolve, reject) {
            try {
              const data = await self._connection.get(self.path, {
                data: { _method: 'GET' },
                headers: {
                  Range: `bytes=${self.start}-${end}`,
                },
              });

              resolve(data);
            } catch (error) {
              reject(error);
            }
          })
        };
        const data = await this._retry.start(requestFn, this);

        if (end === this.fileSize - 1) {
          this.start = null;
        } else {
          this.start = end + 1;
        }

        this.push(data);
      } catch (error) {
        this.emit('error', error);
      }
    } else {
      this.push(null);
    }
  }
}

module.exports = BulkFileStream;
