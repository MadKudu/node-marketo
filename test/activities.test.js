var assert = require('assert'),
  _ = require('lodash'),
  marketo = require('./helper/connection');

describe('Activities', function () {
  this.timeout(5000);
  describe('list activity types', function () {
    it('lists activity types', function (done) {
      marketo.activities
        .getActivityTypes()
        .then(function (resp) {
          var activity = resp.result[0];
          assert.equal(activity.id, 1);
          assert.equal(activity.name, 'Visit Webpage');
          assert(_.has(activity, 'primaryAttribute'));
          done();
        })
        .catch(done);
    });
  });
});
