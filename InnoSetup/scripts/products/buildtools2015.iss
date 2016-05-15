// requires Windows 10, Windows 7 Service Pack 1, Windows 8, Windows 8.1, Windows Server 2003 Service Pack 2, Windows Server 2008 R2 SP1, Windows Server 2008 Service Pack 2, Windows Server 2012, Windows Vista Service Pack 2, Windows XP Service Pack 3
// http://www.microsoft.com/en-US/download/details.aspx?id=48145

[CustomMessages]
buildtools2015_title=Microsoft Build Tools 2015

en.buildtools2015_size=24.5 MB
de.buildtools2015_size=24,5 MB

// {D1437F51-786A-4F57-A99C-F8E94FBA1BD8} Microsoft Build Tools 14.0 (x86) 14.0.23107

// {B8B46064-829B-48A2-A024-3C4DBE91A31F} Microsoft Build Tools 14.0 (amd64) 14.0.12107.10
// {0D76A457-3759-4E8E-8F09-0D8A5C8A6A40} Microsoft Build Tools 14.0 (x86) 14.0.12107.10

// {31FFFC1B-494E-4FF9-9D49-53ACCACB80FD} Microsoft Build Tools 14.0 (amd64)
// {118E863A-F6E9-4A5B-8C61-56B8B752A200} Microsoft Build Tools 14.0 (x86)

// {38368B5D-626E-41C1-A160-CB24B0BCE43C} Microsoft Build Tools 14.0 (amd64)
// {F86F966D-0332-4444-B4D0-FAE76B58D61F} Microsoft Build Tools 14.0 (x86)

// {2BDE4E1E-FE85-471C-8419-35CC61408E27} Microsoft Build Tools 14.0 (amd64)
// {477F7BAD-67AD-4E4F-B704-4AF4F44CB9BD} Microsoft Build Tools 14.0 (x86)

// {165C53A6-4B2E-4BE2-89BF-75D2952DE243} Microsoft Build Tools 14.0 (amd64) 14.0.24730

//{DF27D91D-516E-4DA1-92AC-7D7D59B2D99E} x86
//{7F017105-282F-4091-B16A-F8B8A69B0325} x64

[Code]
const
	buildtools2015_url = 'http://download.microsoft.com/download/E/E/D/EEDF18A8-4AED-4CE0-BEBE-70A83094FC5A/BuildTools_Full.exe';
	
var 
    missing: Boolean;

procedure buildtools2015();
begin
		missing := True;
		
		if msiproduct('{B8B46064-829B-48A2-A024-3C4DBE91A31F}') or 
		   msiproduct('{0D76A457-3759-4E8E-8F09-0D8A5C8A6A40}') or 
		   msiproduct('{D1437F51-786A-4F57-A99C-F8E94FBA1BD8}') or 
		   msiproduct('{118E863A-F6E9-4A5B-8C61-56B8B752A200}') or 
		   msiproduct('{F86F966D-0332-4444-B4D0-FAE76B58D61F}') or 
		   msiproduct('{477F7BAD-67AD-4E4F-B704-4AF4F44CB9BD}') or
		   msiproduct('{165C53A6-4B2E-4BE2-89BF-75D2952DE243}') or
		   msiproduct('{DF27D91D-516E-4DA1-92AC-7D7D59B2D99E}') or
		   msiproduct('{7F017105-282F-4091-B16A-F8B8A69B0325}') then 
		begin
			missing := False;
		end;

		if (missing) then
		begin
			AddProduct('BuildTools_Full.exe', '/passive /norestart',
				CustomMessage('buildtools2015_title'),
				CustomMessage('buildtools2015_size'),
				buildtools2015_url,
				false, false);
		end;
end;
