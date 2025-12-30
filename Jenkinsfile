@Library('devprod-cloudbees-shared-libraries') _

pipeline {
    agent any
    
    stages {
        stage('Prepare') {
            steps {
                sh 'cat ${DEVPROD_SSL_CERT_ARTIFACTORY_DIR}/devprod_ro.crt > devprod_artifactory_read_combined.pem'
                sh 'echo "" >> devprod_artifactory_read_combined.pem'
                sh 'cat ${DEVPROD_SSL_CERT_ARTIFACTORY_DIR}/devprod_ro.key >> devprod_artifactory_read_combined.pem'
                sh 'cat ${DEVPROD_SSL_CERT_ARTIFACTORY_DIR}/devprod_ro.ca > devprod_ca.pem'
                sh 'ls -al'
            }
        }
        
        stage('Build') {
            agent {
                docker {
                    image 'docker.akamai.com/microsoft-mcr-upstream/dotnet/sdk:8.0'
                    reuseNode true
                    args '--user 0:0'
                }
            }
            steps {
                sh 'ls -al'
                
                // Install CA certificate
                sh 'cp devprod_ca.pem /usr/local/share/ca-certificates/devprod_ca.crt'
                sh 'update-ca-certificates'
                
                // Create certificate directories
                sh 'mkdir -p /root/.certs/'
                sh 'cp devprod_artifactory_read_combined.pem /root/.certs/'
                sh 'cp devprod_ca.pem /root/.certs/'
                
                // Create PKCS#12 bundle for NuGet
                sh '''
                    CERT_PFX_PATH=/root/.certs/devprod_ro.pfx
                    CERT_PASSWORD="edgegrid-ci"
                    openssl pkcs12 -export \
                        -in /root/.certs/devprod_artifactory_read_combined.pem \
                        -certfile /root/.certs/devprod_ca.pem \
                        -out ${CERT_PFX_PATH} \
                        -passout pass:${CERT_PASSWORD}
                    chmod 600 ${CERT_PFX_PATH}
                '''
                
                // Configure NuGet
                sh '''
                    cat > $WORKSPACE/NuGet.Config <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="akamai-artifactory" value="https://repos.akamai.com/artifactory/api/nuget/v3/nuget-upstream/index.json" />
    </packageSources>
</configuration>
EOF
                '''
                
                // Add client certificate to NuGet
                sh '''
                    dotnet nuget add client-cert \
                        --package-source akamai-artifactory \
                        --path /root/.certs/devprod_ro.pfx \
                        --password edgegrid-ci \
                        --store-password-in-clear-text \
                        --configfile $WORKSPACE/NuGet.Config
                '''
                
                // Restore and build
                sh 'dotnet restore --verbosity normal'
                sh 'chmod +x scripts/ci.sh'
                sh './scripts/ci.sh --coverage'
            }
        }
        
        stage('Scan') {
            steps {
                script {

                    runOsvScan(
                        projectName: 'akamaiopen-edgegrid-c-sharp', 
                        projectVersion: '1.0.0', 
                        tokenCredsId: 'devexp-bd-token'
                    )
                }
            }
        }
    }
    
    post {
        always {
            script {
                // Publish JUnit test results
                junit testResults: '**/reports/test-results/junit-results.xml',
                      allowEmptyResults: false,
                      healthScaleFactor: 1.0
                
                // Publish coverage using Code Coverage API Plugin
                // recordCoverage(tools: [[parser: 'COBERTURA', pattern: 'reports/coverage/*.xml']])

                // Archive all artifacts
                archiveArtifacts artifacts: 'reports/**/*',
                                 allowEmptyArchive: true,
                                 fingerprint: true
            }
        }
        
        success {
            echo 'Build and tests passed successfully!'
        }
        
        failure {
            echo 'Build or tests failed!'
        }
    }
}
