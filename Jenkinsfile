@Library('hobom-shared-lib') _
hobomPipeline(
  serviceName:    'dev-hammer-auction',
  hostPort:       '5002',
  containerPort:  '8080',
  memory:         '512m',
  cpus:           '0.5',
  envPath:        '/etc/hobom-dev/dev-hammer-auction/.env',
  addHost:        true,
  submodules:     false,
  smokeCheckPath: '/health'
)
