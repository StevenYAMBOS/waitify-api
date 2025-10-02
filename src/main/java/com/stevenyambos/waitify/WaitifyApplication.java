package com.stevenyambos.waitify;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.jdbc.DataSourceAutoConfiguration;

//@SpringBootApplication(scanBasePackages = {"controllers","services","models", "repositories"})
//@SpringBootApplication(exclude = { DataSourceAutoConfiguration.class })
@SpringBootApplication
public class WaitifyApplication {

	public static void main(String[] args) {
		SpringApplication.run(WaitifyApplication.class, args);
	}

}
