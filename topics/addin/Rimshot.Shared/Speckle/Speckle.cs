using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;
using Speckle.Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Rimshot.SpeckleApi {
  internal class SpeckleServer {
    internal string rimshotIssueId;

    public string HostUrl { get; set; }  // TODO: Feedback to the UI when the Client Account does not match the HostUrl. Handle the ability to Commit.
    public string StreamId { get { if ( this.Stream != null ) { return Stream.id; } return null; } }
    public string BranchName { get { if ( Branch != null ) { return Branch.name; } return null; } }
    public string CommitId { get; set; }
    public Account Account { get; set; }
    public Client Client { get; set; }
    public Branch Branch { get; set; }
    public Stream Stream { get; set; }

    public Bindings.DefaultBindings App { get; set; }

    public SpeckleServer () {
      Account defaultAccount = AccountManager.GetDefaultAccount();

      if ( defaultAccount == null ) {
        Logging.ErrorLog( new SpeckleException( "You do not have any accounts active. Please create one or add it to the Speckle Manager." ), null );
        return;
      }

      this.Account = defaultAccount;

      try {
        Client = new Client( this.Account ); // TODO: Feedback to the UI when the Client Account does not match the HostUrl.
      } catch ( Exception e ) {
        Logging.ErrorLog( e );
      }
    }

    public async Task TryGetStream ( string streamId ) {
      this.Stream = null;
      try {
        Stream = await Client.StreamGet( streamId );
      } catch {
        Logging.ErrorLog( new SpeckleException( $"You don't have access to stream {streamId} on server {this.HostUrl}, or the stream does not exist." ), this.App );
      }
    }

    private Task<Branch> CreateBranch ( string name, string description ) {

      try {
        this.Client.BranchCreate( new BranchCreateInput() {
          streamId = this.StreamId,
          name = name,
          description = description
        } );
      } catch ( Exception e ) {
        Logging.ErrorLog( e );
      }

      Task<Branch> branch = Client.BranchGet( this.StreamId, name );

      return branch;
    }


    public async Task TryGetBranch ( string name, string description = "" ) {
      this.Branch = null;

      // Get Branch and create if it doesn't exist.
      try {
        this.Branch = await Client.BranchGet( this.StreamId, name );
        if ( Branch is null ) {
          try {
            Branch = await CreateBranch( name, description );
          } catch ( Exception ) {
            Logging.ErrorLog( new SpeckleException( $"Unable to find an issue branch for {BranchName}" ), App );
          }
        }
      } catch ( Exception ) {
        Logging.ErrorLog( new SpeckleException( $"Unable to find or create an issue branch for {this.BranchName}" ), App );
        return;
      }

      if ( Branch == null ) {
        try {
          Branch = Client.BranchGet( this.StreamId, this.BranchName, 1 ).Result;

          if ( Branch != null ) {
            App.NotifyUi( "branch_updated", JsonConvert.SerializeObject( new { branch = Branch.name, rimshotIssueId } ) );
          }
        } catch ( Exception ) {
          Logging.ErrorLog( new SpeckleException( $"Still unable to find an issue branch for {BranchName}" ), App );
        }
      }
    }
  }
}
