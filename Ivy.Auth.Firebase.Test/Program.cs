using Ivy;
using Ivy.Auth.Firebase;
using Ivy.Auth.Firebase.Test.Apps;

Server.Start(new FirebaseHelloApp(), new FirebaseAuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub());

Console.WriteLine("Firebase Auth Test App running on http://localhost:5010");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();