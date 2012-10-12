<?php 
function parse_signed_request($signed_request, $secret) {
  list($encoded_sig, $payload) = explode('.', $signed_request, 2); 

  // decode the data
  $sig = base64_url_decode($encoded_sig);
  $data = json_decode(base64_url_decode($payload), true);

  if (strtoupper($data['algorithm']) !== 'HMAC-SHA256') {
    error_log('Unknown algorithm. Expected HMAC-SHA256');
    return null;
  }

  // check sig
  $expected_sig = hash_hmac('sha256', $payload, $secret, $raw = true);
  if ($sig !== $expected_sig) {
    error_log('Bad Signed JSON signature!');
    return null;
  }

  return $data;
}

function base64_url_decode($input) {
  return base64_decode(strtr($input, '-_', '+/'));
}

$app = array(
	"id" => "223453524434397",
	"secret" => "3b6769b53e52b0d122d2572950d770bd",
	"namespace" => "playwithyourpeas",
	"permissions" => "");

$signed_request = parse_signed_request($_REQUEST['signed_request'], $app["secret"]);

if (isset($signed_request["user_id"]))
	define("Authorized", $signed_request["user_id"]);
if (isset($_POST["error_reason"]) && $_POST["error_reason"] == "user_denied")
	define("Denied", true);


?>
<!doctype html>
<!--[if lt IE 7]> <html class="no-js lt-ie9 lt-ie8 lt-ie7" lang="en"> <![endif]-->
<!--[if IE 7]>    <html class="no-js lt-ie9 lt-ie8" lang="en"> <![endif]-->
<!--[if IE 8]>    <html class="no-js lt-ie9" lang="en"> <![endif]-->
<!--[if gt IE 8]><!--> <html class="no-js" lang="en"> <!--<![endif]-->
<head>
	<meta charset="utf-8">
	<meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
    <meta property="og:type"        content="game" /> 
 	<meta property="og:url"         content="http://peas.derk-jan.org/canvas/"> 
  	<meta property="og:title"       content="Play With Your Peas" /> 
  	<meta property="og:image"       content="http://peas.derk-jan.org/img/Identifier.png" /> 
  	<meta property="og:description" content="Play With Your Peas - Prototype Challenge Game" />

	<title>Play With Your Peas - Canvas</title>
	<meta name="description" content="">
	<meta name="author" content="">

	<meta name="viewport" content="width=device-width">
	<link href="/less/style.css" rel="stylesheet">

	<script src="/js/libs/modernizr-2.5.3-respond-1.1.0.min.js"></script>
    <style>
    
        html, body {
	        height: 100%;
	        overflow: none;
        }
        
        body {
	        padding: 0;
	        margin: 0;
	        background-color: Gray;
        }
        
        #silverlight, #silverlightControlHost {
	        height: 99%;
	        text-align:center;
	        margin: 0;
        }
        
        #silverlightControlHost object {
            width: 100%;
            height: 99%;
        }

		.container > .hero-unit { 
			margin-top: 20px;
		}
    
    </style>
</head>
<body>
<!--[if lt IE 7]><p class=chromeframe>Your browser is <em>ancient!</em> <a href="http://browsehappy.com/">Upgrade to a different browser</a> or <a href="http://www.google.com/chromeframe/?redirect=true">install Google Chrome Frame</a> to experience this site.</p><![endif]-->

    <div id="fb-root"></div>
    <script>
        window.fbAsyncInit = function () {
            FB.init({
                appId: '223453524434397', // App ID
                channelUrl: '//www.derk-jan.org/files/canvas/channel.html', // Channel File
                status: true, // check login status
                cookie: true, // enable cookies to allow the server to access the session
                xfbml: true  // parse XFBML
            });

            // Additional initialization code here
            /*FB.login(function (response) {
                if (response.authResponse) {
                    console.log('Welcome!  Fetching your information.... ');
                    FB.api('/me', function (response) {
                        console.log('Good to see you, ' + response.name + '.');
                    });
                } else {
                    console.log('User cancelled login or did not fully authorize.');
                }
            });

            FB.Canvas.getPageInfo(
                function (info) {
                    console.log('Width: ' + info.clientWidth + ' Height: ' + info.clientHeight);
                }
            );*/
        };

        // Load the SDK Asynchronously
        (function (d) {
            var js, id = 'facebook-jssdk', ref = d.getElementsByTagName('script')[0];
            if (d.getElementById(id)) { return; }
            js = d.createElement('script'); js.id = id; js.async = true;
            js.src = "//connect.facebook.net/en_US/all.js";
            ref.parentNode.insertBefore(js, ref);
        } (document));
    </script>

	<?php 
	if (!defined("Authorized")) { ?>
	
	<div class="container">
    <!-- Marketing -->
        <div class="hero-unit">
            <h1>Feel like a Ninja?</h1>
            <p>You have a bunch of fun loving sentient peas that like to climb tall objects and jump off 
            of them. They think they are ninjas. You know they are suicidal.
            Can you build a brilliant playground that delights your peas?</p>
            <p>
                <a class="btn btn-primary btn-large" target="_top" 
                    href="
                        https://www.facebook.com/dialog/oauth/?
                            client_id=<?php echo $app["id"]; ?>
                            &redirect_uri=https%3A%2F%2Fapps.facebook.com%2F<?php echo $app["namespace"]; ?>%2F
                            &scope=<?php echo $app["permissions"]; ?>"
                >
                Authenticate Me!
                </a>
        
                <?php if (defined("Denied")) { ?>
                    <span class="alert">You denied!</span>
                <?php } ?>
            </p>
        </div>
	</div>

	<?php } else { ?>
    <div class="container">
		<div class="modal fade" id="gpumessage">
			<div class="modal-header">
				<button class="close" data-dismiss="modal">×</button>
				<h3>GPU Acceleration</h3>
			</div>
            
			<div class="modal-body">
				<p>If your get the message to enable <b>GPU acceleration</b>, right click on the gray background and pick <b>Silverlight</b> from the context menu. Under permissions allow this application to use the GPU and enable GPU under the Playback tab. Then <a href="/canvas/" title="Reload" class="btn">reload</a> the page.</p>
			</div>
		</div>
	</div>

    <form id="silverlight" runat="server">
        <div id="silverlightControlHost">
            <object data="data:application/x-silverlight-2," type="application/x-silverlight-2">
		        <param name="source" value="../silverlight/PlayWithYourPeas.SilverLight.App.xap"/>
                <param name="initParams" value="Token=<?php echo $signed_request["oauth_token"] ?>"/>
		        <param name="onError" value="onSilverlightError" />
		        <param name="background" value="gray" />
		        <param name="minRuntimeVersion" value="5.0.60401.0" />
		        <param name="autoUpgrade" value="true" />
                <param name="enableGPUAcceleration" value="true" />
		        <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.60401.0" style="text-decoration:none">
 			        <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style:none"/>
		        </a>
	        </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>
        </div>
    </form>
	<?php } ?>

    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js"></script>
    <script>        window.jQuery || document.write('<script src="js/libs/jquery-1.7.1.min.js"><\/script>')</script>

    <script src="/js/libs/bootstrap/transition.js"></script>
    <script src="/js/libs/bootstrap/collapse.js"></script>
    <script src="/js/libs/bootstrap/alert.js"></script>
    <script src="/js/libs/bootstrap/modal.js"></script>

    <script src="/js/script.js"></script>
    <script>        $("#gpumessage").modal('show');</script>
</body>
</html>
