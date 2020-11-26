# pokemon-shakespeare

An Api for retrieving a shakespearian (Shakespearean?) representation of Pokemon/Pokemen 

Can be run from visual studio (F5 should work), or the command line. (dotnet run should work, provided you're in the context of the `PokemonShakespeareApi` folder.)

There are some unit tests which can be run from Visual Studio if you're lucky enough to have ReSharper, or Jetbrains Rider. Alternatively, dotnet test from the command line should work, from the context of the  `PokemonShakespeareApi.Tests` folder

You can call it with postman, or curl, or any flavour of thing that does http calls, make a request to (for example) https://localhost:5001/api/pokemon/translation/pikachu 

## Notes

I've added a naive caching mechanism, with a configurable expiry. This implementation would suffer from potential concurrency issues as it employs no locking, so that would be an improvement to the system. 

It will also cache the 429 (too many requests) response from the pokemon api, as it is quite restrictive - this is another thing that I would change if I had more time.

Unfortunately I ran out of time for adding a usable dockerfile, but my git commit history should be visible on github as the repository is (I hope) public.

I would have also liked to have written some more tests, being able to assert that a value came from the cache rather than the remote api - but this proved slightly more of a challenge to proxy/stub than I thought, so I ran out of time a little bit here too.

Thanks for reading.
Matt