************************************************************************************************************************

										MEBMERSUITE INTEGRATION LINK EXAMPLE CODE

************************************************************************************************************************

MemberSuite uses Single Sign On (SSO) technology to allow the integration link code, which resides outside of MemberSuite
and in the insecure Internet, to assume to role of the currently logged in user. One of the requirements of this
arrangement is that the integration link must be hosted behind an SSL site; otherwise, the SSO key could be intercepted
and represented back to the API in a classic "man in the middle" attack. As such, you won't be able to run this site
using Visual Studio's hosted HTTP service. You will need to host it in IIS with a self signed certificate.

To get this integration link up and running:

1. In IIS, create a self-signed certificate named "localhost". You can do this by clicking on the server and selecting
   Server Certificates, and then clicking "Create Self-Signed Certificate"
2. Create a new site in IIS, and point it to the directory of this example. Use an https binding, and use the self-signed
   certificate you just created
3. Browse to https://localhost. You should see the integration link.


Once you've done this, you're ready to setup the integration link. The first link you'll set up will be:

 https://localhost/Default.aspx

You should make this either a Module Homepage or Tab integration link. The second will be:

https://localhost/360ScreenIntegrationLink.aspx

This link is designed to work as a 360° screen link, and has the added feature of receiving the current object's context
and displaying that objects information.