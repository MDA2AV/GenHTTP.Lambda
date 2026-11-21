using System.Net;

using MaxMind.Db;

namespace GenHTTP.Lambda.Services.Diagnostics;

/*
 * Only the fields that end up on a log line are declared. Anything the
 * database holds and these do not name is stepped over rather than decoded,
 * which is where the cost of a lookup went: a city record carries its name in
 * every language the database ships, its region, its continent and a pair of
 * coordinates, and reading two strings off all of that cost forty
 * microseconds and six kilobytes of garbage an address.
 *
 * Internal rather than nested and private, because the reader generates its
 * decoding code against them and cannot see inside a class to do it.
 */

internal sealed class Names
{
    [MapKey("en")]
    public string? English { get; set; }
}
