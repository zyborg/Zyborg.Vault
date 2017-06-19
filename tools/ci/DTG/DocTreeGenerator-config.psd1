# Configuration file for Publish-ModuleDocumentationTree

## NOTE:  THIS IS NOT CURRENTLY IN USE!!!
##        MAY BE DELETED EVENTUALLY IF WE DON'T SWITCH TO IT

@{

# Location of your source control repository containing your modules
ProjectRoot = '.'

# List each module here with name and paths (relative to project root)
Modules = @(
	@{
		Name = 'Zyborg.Vault'
		SourcePath = '..\Debug'
		BinPath = '.'
	}
	#,
	#@{
	#	Name = 'Acme.Glom.Admin.PowerShell'
	#	SourcePath = 'projects\Glom.Tools\src\Acme.Glom.Admin.PowerShell'
	#	BinPath = 'projects\Glom.Tools\output\Debug'
	#}
)

# The parent directory name of all your modules
Namespace = 'Zyborg.Vault'

# Title on each web page
# (note that the Namespace is prefixed to this title)
DocTitle = ' - Zyborg Vault PowerShell API'

# Output directory (relative to your current directory)
DocPath = '.\api-docs'

# HTML template file for DocTreeGenerator (relative to project root)
#TemplatePath = 'projects\Common\doc\psdoc_template.html'

# Location of namespace_overview.html file (relative to project root)
#NamespaceOverviewPath = 'projects\Common\doc'

# Information added to web pages
CopyrightYear = 'Copyright (C) Zyborg.'

# Information added to web pages
RevisionDate = '2017.06.06'

}