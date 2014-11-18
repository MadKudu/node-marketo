# node-marketo

This implements (a subset of) Marketo's REST API.

## Overview

This library is a simple wrapper around Marketo's REST API and does not aim to be anything more than that.

## Creating a connection

You will first need to obtain your OAuth information from Marketo, they have a [guide out](http://developers.marketo.com/documentation/rest/authentication/) to get you started. In short, you will need to get the endpoint url, identity url, client id and client secret.

    var Marketo = require('node-marketo');
    
    var marketo = new Marketo({
      endpoint: 'https://123-ABC-456.mktorest.com/rest',
      identity: 'https://123-ABC-456.mktorest.com/identity',
      clientId: 'client id',
      clientSecret: 'client secret'
    });
    
    marketo.lead.find('id', [2, 3])
      .spread(function(data, resp) {
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


## Implemented API


### Lead

#### lead.byId(id, options)

Implements [Get Lead by Id](http://developers.marketo.com/documentation/rest/get-lead-by-id/)

param | type | description
------|------|------------
`id`  | int | the lead id to query for
`options` | object | `fields`: a comma separated list or array of fields to retrieve

    marketo.lead.byId(3)
      .spread(function(data) {
        console.log(data);
      })
    
    // Using the field attribute
    marketo.lead.byId(3, ['email', 'lastName'])
      .spread(function(data) {
        // data.result[0]
        //
        // {
        //   email: "some@email.com",
        //   lastName: "LastName"
        // }
      });


#### lead.find(filterType, filterValues, options)

Implements [Get Multiple Leads by Filter Type](http://developers.marketo.com/documentation/rest/get-multiple-leads-by-filter-type/)

param | type | description
------|------|------------
`filterType`  | string | the field that we will filter on
`filterValues`  | Array<string>/string | the values that we will filter for
`options` | object | `fields`: a comma separated list or array of fields to retrieve
|| `batchSize`: the number of lead records to be returned (max is 300)
|| `nextPageToken`: used to paginate through large result sets

    marketo.lead.find('email', ['email@one.com', 'email@two.com'])
    
    // or
    marketo.lead.find('email', 'email@one.com,email@two.com')


#### lead.createOrUpdate(input, options)

Implements [Create/Update Leads](http://developers.marketo.com/documentation/rest/createupdate-leads/)

param | type | description
------|------|------------
`input`  | Array<Object> | An array of lead records to create or update
`options` | object | `action`: one of 4 valid actions (createOnly, updateOnly, ...)
|| `lookupField`: the field used to dedup on
|| `partitionName`: not sure what this does yet, :)

    // Since the action is not passed in, the default action is 'createOrUpdate'
    marketo.lead.createOrUpdate(
        [{'email': 'email@one.com'}, {'email': 'email@two.com'}],
        {lookupField: 'email'}
      )
    
    // The same query without creating new leads
    marketo.lead.createOrUpdate(
        [{'email': 'email@one.com'}, {'email': 'email@two.com'}],
        {lookupField: 'email', action: 'updateOnly'}
      )

### List

#### list.all()
