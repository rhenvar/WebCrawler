﻿<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="WebCrawler" generation="1" functional="0" release="0" Id="d7978060-d33d-4de7-b9a8-c278b5528c55" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="WebCrawlerGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="CrawlerWebRole:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/WebCrawler/WebCrawlerGroup/LB:CrawlerWebRole:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="CrawlerWebRole:APPINSIGHTS_INSTRUMENTATIONKEY" defaultValue="">
          <maps>
            <mapMoniker name="/WebCrawler/WebCrawlerGroup/MapCrawlerWebRole:APPINSIGHTS_INSTRUMENTATIONKEY" />
          </maps>
        </aCS>
        <aCS name="CrawlerWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/WebCrawler/WebCrawlerGroup/MapCrawlerWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="CrawlerWebRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/WebCrawler/WebCrawlerGroup/MapCrawlerWebRoleInstances" />
          </maps>
        </aCS>
        <aCS name="CrawlerWorkerRole:APPINSIGHTS_INSTRUMENTATIONKEY" defaultValue="">
          <maps>
            <mapMoniker name="/WebCrawler/WebCrawlerGroup/MapCrawlerWorkerRole:APPINSIGHTS_INSTRUMENTATIONKEY" />
          </maps>
        </aCS>
        <aCS name="CrawlerWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/WebCrawler/WebCrawlerGroup/MapCrawlerWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="CrawlerWorkerRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/WebCrawler/WebCrawlerGroup/MapCrawlerWorkerRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <sFSwitchChannel name="IE:CrawlerWorkerRole:WorkerEndpoint">
          <toPorts>
            <inPortMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWorkerRole/WorkerEndpoint" />
          </toPorts>
        </sFSwitchChannel>
        <lBChannel name="LB:CrawlerWebRole:Endpoint1">
          <toPorts>
            <inPortMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRole/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapCrawlerWebRole:APPINSIGHTS_INSTRUMENTATIONKEY" kind="Identity">
          <setting>
            <aCSMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRole/APPINSIGHTS_INSTRUMENTATIONKEY" />
          </setting>
        </map>
        <map name="MapCrawlerWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapCrawlerWebRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRoleInstances" />
          </setting>
        </map>
        <map name="MapCrawlerWorkerRole:APPINSIGHTS_INSTRUMENTATIONKEY" kind="Identity">
          <setting>
            <aCSMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWorkerRole/APPINSIGHTS_INSTRUMENTATIONKEY" />
          </setting>
        </map>
        <map name="MapCrawlerWorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWorkerRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapCrawlerWorkerRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWorkerRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="CrawlerWebRole" generation="1" functional="0" release="0" software="C:\Users\rhenvar\Documents\Visual Studio 2015\Projects\WebCrawler\WebCrawler\csx\Release\roles\CrawlerWebRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="APPINSIGHTS_INSTRUMENTATIONKEY" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;CrawlerWebRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;CrawlerWebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;CrawlerWorkerRole&quot;&gt;&lt;e name=&quot;WorkerEndpoint&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
        <groupHascomponents>
          <role name="CrawlerWorkerRole" generation="1" functional="0" release="0" software="C:\Users\rhenvar\Documents\Visual Studio 2015\Projects\WebCrawler\WebCrawler\csx\Release\roles\CrawlerWorkerRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <settings>
              <aCS name="APPINSIGHTS_INSTRUMENTATIONKEY" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;CrawlerWorkerRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;CrawlerWebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;CrawlerWorkerRole&quot;&gt;&lt;e name=&quot;WorkerEndpoint&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWorkerRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWorkerRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWorkerRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="CrawlerWebRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyUpdateDomain name="CrawlerWorkerRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="CrawlerWebRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyFaultDomain name="CrawlerWorkerRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="CrawlerWebRoleInstances" defaultPolicy="[1,1,1]" />
        <sCSPolicyID name="CrawlerWorkerRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="93183c2e-9acc-4600-a2bd-6f635a4e11bc" ref="Microsoft.RedDog.Contract\ServiceContract\WebCrawlerContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="2048cf3f-4f8e-4b62-965f-19ca8bd58819" ref="Microsoft.RedDog.Contract\Interface\CrawlerWebRole:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/WebCrawler/WebCrawlerGroup/CrawlerWebRole:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>