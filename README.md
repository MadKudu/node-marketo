# node-marketo

A wrapper around Marketo's REST API.

## Usage

### Installation

```
npm install --save node-marketo-rest
```

### Creating a connection

You will first need to obtain your OAuth information from Marketo, they have a [guide out](http://developers.marketo.com/documentation/rest/authentication/) to get you started. In short, you will need to get the endpoint url, identity url, client id and client secret.

```js
var Marketo = require('node-marketo-rest');

var marketo = new Marketo({
  endpoint: 'https://123-ABC-456.mktorest.com/rest',
  identity: 'https://123-ABC-456.mktorest.com/identity',
  clientId: 'client id',
  clientSecret: 'client secret'
});

marketo.lead.find('id', [2, 3])
  .then(function(data, resp) {
    // data is:
    // {
    //   requestId: '17787#149c01d54b8',
    //   result: [{
    //     id: 2,
    //     updatedAt: '2014-11-13 15:25:36',
    //     createdAt: '2014-11-13 15:25:36',
    //     ...
    //   }, {
    //     id: 3,
    //     updatedAt: '2014-11-13 16:22:03',
    //     createdAt: '2014-11-13 16:22:03',
    //     ...
    //   }],
    //   success: true
    // }
  });
```

### Pagination

When a specific call results in a lot of elements, you will have to paginate to get all of the data. For example, getting all the leads in a large list will likely exceed the maximum batch size of 300. When this happens, you can check for the existence of the `nextPageToken` and use it in the next call:

```js
marketo.list.getLeads(1)
  .then(function(data) {
    if (data.nextPageToken) {
      // preserve the nextPageToken for use in the next call
    }
  });
```

If you want a less manual process, the result comes with a convenient `nextPage` function that you can use. This function only exists if there's additional data:

```js
marketo.list.getLeads(1)
  .then(function(page1) {
    // do something with page1

    if (page1.nextPageToken) {
      return page1.nextPage();
    }
  })
  .then(function(page2) {
    // do something with page2
  });
```

### Stream

Instead of getting data back inside of the promise, you can also turn the response into a stream by using the `Marketo.streamify` function. This will emit the results one element at a time. If the result is paginated, it will lazily load all of the pages until it's done:

```js
return marketo.streamify('lead', 'getLeads', [ 1 ])

resultStream
  .on('data', function(lead) {
    // do something with lead
  })
  .on('error', function(err) {
    // log the list error. Note, the stream closes if it encounters an error
  })
  .on('end', function() {
    // end of the stream
  });
```

```js
var count = 0;

resultStream
  .on('data', function(data) {
    if (++count > 20) {
      // Closing stream, this CAN be called multiple times because the
      // buffer of the queue may already contain additional data
      resultStream.endMarketoStream();
    }
  })
  .on('end', function() {
    // count here CAN be more than 20
    console.log('done, count is', count);
  });
```

### Extract

Bulk Extract provides a programmatic interface for retrieving large amounts of activity data out of Marketo.  For cases which do not require low latency, and need to transfer significant volumes of activity data out of Marketo, such as CRM-integration, ETL, data warehousing, and data archiving.

The process requires you to `bulkLeadExtract.create` a request, then `bulkLeadExtract.enqueue` the request, then poll the `bulkLeadExtract.status`, `bulkLeadExtract.statusTilCompleted` will poll till completed, also while calling `bulkLeadExtract.cancel` for any errors.

The `bulkLeadExtract.get` method will preform these all these actions for you.

Then `bulkLeadExtract.file` or `bulkLeadExtract.fileStream` can be used to access the extracted file.

Bulk Extract Leads
```js
marketo.bulkLeadExtract.get(
  ['firstName', 'lastName', 'id', 'email'],
  { staticListName: 'some list name' })
  .then(function(data) {
    // export is ready
    let exportId = data.result[0].exportId;
    return marketo.bulkLeadExtract.file(exportId);
  })
  .then(function(file) {
    // do something with file
  });
```

####Bulk Activity Extract
Follow the same convention as bulk extracting leads, but provide different filter information for the start/end date, activity type ids and any other supported filters or parameters. [Marketo Docs] (http://developers.marketo.com/rest-api/bulk-extract/bulk-activity-extract/)


```
marketo.bulkActivityExtract.get(
        {createdAt: {
                startAt: '2018-07-01T23:59:59-00:00',
                endAt:  '2018-07-03T23:59:59-00:00'
            },
            activityTypeIds: [
                1, //example types
                12,
                13
            ]})
        .then(function(data) {
            // export is ready
            let exportId = data.result[0].exportId;
            return marketo.bulkActivityExtract.file(exportId);
        })
        .then(function(file) {
            //Do something with file
        });
});
```


# Test

### Generating a replay for a test

The initial run of the test has to be run against an actual API, the run (when
in recording mode) should capture the API request/response data, which will then
be used for future calls. What makes this a little tricky is Marketo API
endpoints are unique per account, so we have to convert the captured data to
something else for future purposes. We also need to remove credentials from
these captures as well. Anyway, here's the annoying process of generating data:

##### Running against the actual API

The test looks at 4 environment variables when running, they are:

- `MARKETO_ENDPOINT`
- `MARKETO_IDENTITY`
- `MARKETO_CLIENT_ID`
- `MARKETO_CLIENT_SECRET`

After setting these variables, run `npm run testRecord`.

##### Stripping sensitive information

Run the script `./scripts/strip-fixtures.sh`, which will convert the unique
Marketo host over to `123-abc-456.mktorest.com` in addition to removing the
capture that contains sensitive client id/secret file.

##### Copy the data

The data should now be moved over to `fixtures/123-abc-456.mktorest.com-443`,
preferably with a useful name so it's easier to keep track of in the future.

##### Tip

We are using mocha to run the test, you can run a single test so that you
generate only the needed data. To do so, you append `only` to a test:

```js
    // on a single unit test
    it.only('test description', function() {})

    // or an entire describe
    describe.only('test description', function() {})
```

One more thing to note is that once we've processed the raw data, `node-replay`
will not be able to map it back to the original raw request. This means that if
you run `npm run testRecord`, you will be generating requests against Marketo's
API directly. I highly recommend using `only`.
