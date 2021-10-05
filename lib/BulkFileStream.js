const { Readable } = require('stream');

const DEFAULT_RANGE_SIZE = 750000; // 750 KB

class BulkFileStream extends Readable {
  constructor(_connection, path, fileSize, rangeSize) {
    super();
    this._connection = _connection;
    this.path = path;
    this.start = 0;
    this.fileSize = fileSize;
    this.rangeSize = rangeSize || DEFAULT_RANGE_SIZE;
  }

  async _read() {
    if (this.start !== null) {
      let end = this.start + this.rangeSize;
      if (end >= this.fileSize) {
        end = this.fileSize - 1;
      }

      const data = await this._connection.get(this.path, {
        data: { _method: 'GET' },
        headers: {
          Range: `bytes=${this.start}-${end}`,
        },
      });

      if (end === this.fileSize - 1) {
        this.start = null;
      } else {
        this.start = end + 1;
      }

      this.push(data);
    } else {
      this.push(null);
    }
  }
}

module.exports = BulkFileStream;
