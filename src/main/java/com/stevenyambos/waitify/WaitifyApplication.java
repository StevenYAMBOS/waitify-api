package com.stevenyambos.waitify;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.jdbc.DataSourceAutoConfiguration;

@SpringBootApplication(exclude = { DataSourceAutoConfiguration.class })
public class WaitifyApplication {

	public static void main(String[] args) {
		SpringApplication.run(WaitifyApplication.class, args);
	}

}
